using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos.Auditoria;

namespace PROmaderas.AccesoADatos.Parametros
{
    public class ParametroPlanillaRepositorio : IParametroPlanillaRepositorio
    {
        private const string Tabla = "ParametroPlanilla";

        private readonly Contexto _contexto;

        public ParametroPlanillaRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        // ── Consumo del cálculo ───────────────────────────────────────────────

        public async Task<decimal?> ObtenerValorVigente(string nombre, DateTime fecha)
        {
            var version = await ObtenerVersionVigente(nombre, fecha);
            return version?.Valor;
        }

        public async Task<Dictionary<string, decimal>> ObtenerVigentes(DateTime fecha)
        {
            var f = fecha.Date;

            // UNA sola query: se traen todas las versiones que cubren la fecha y se agrupan
            // en memoria (son pocas filas). El cálculo de planilla no puede darse el lujo de
            // pegarle a la BD una vez por parámetro.
            var vigentes = await _contexto.ParametrosPlanilla
                .Where(p => p.Estado
                         && f >= p.FechaInicio
                         && (p.FechaFin == null || f <= p.FechaFin))
                .ToListAsync();

            // Si por datos sucios dos versiones se solaparan, gana la de FechaInicio más reciente.
            return vigentes
                .GroupBy(p => p.NombreParametro)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(p => p.FechaInicio).First().Valor);
        }

        // ── Consultas de pantalla ─────────────────────────────────────────────

        public async Task<List<ParametroConVigenciaAD>> ObtenerParametrosConVigente(DateTime fecha)
        {
            var f = fecha.Date;

            // Se traen todas las filas (son ~17 parámetros con pocas versiones cada uno) y se
            // agrupa por nombre: así los parámetros SIN versión vigente igual aparecen listados.
            var todas = await _contexto.ParametrosPlanilla.ToListAsync();

            return todas
                .GroupBy(p => p.NombreParametro)
                .Select(g => new ParametroConVigenciaAD
                {
                    NombreParametro = g.Key,
                    TotalVersiones = g.Count(),
                    Vigente = g
                        .Where(p => p.Estado
                                 && f >= p.FechaInicio
                                 && (p.FechaFin == null || f <= p.FechaFin))
                        .OrderByDescending(p => p.FechaInicio)
                        .FirstOrDefault()
                })
                .OrderBy(x => x.NombreParametro)
                .ToList();
        }

        public async Task<List<ParametroPlanillaAD>> ObtenerHistorial(string nombre)
            => await _contexto.ParametrosPlanilla
                .Where(p => p.NombreParametro == nombre)
                .OrderByDescending(p => p.FechaInicio)
                .ThenByDescending(p => p.IdParametroPlanilla)
                .ToListAsync();

        public async Task<ParametroPlanillaAD?> ObtenerVersionVigente(string nombre, DateTime fecha)
        {
            var f = fecha.Date;

            // La consulta de resolución de la HU. Estado == true: una versión anulada no rige
            // nunca; una versión CERRADA (FechaFin != null) sí rige para su propio período.
            return await _contexto.ParametrosPlanilla
                .Where(p => p.NombreParametro == nombre
                         && p.Estado
                         && f >= p.FechaInicio
                         && (p.FechaFin == null || f <= p.FechaFin))
                .OrderByDescending(p => p.FechaInicio)
                .FirstOrDefaultAsync();
        }

        public async Task<ParametroPlanillaAD?> ObtenerVersionPorId(int idVersion)
            => await _contexto.ParametrosPlanilla
                .FirstOrDefaultAsync(p => p.IdParametroPlanilla == idVersion);

        public async Task<ParametroPlanillaAD?> ObtenerUltimaVersionActiva(string nombre)
            => await _contexto.ParametrosPlanilla
                .Where(p => p.NombreParametro == nombre && p.Estado)
                .OrderByDescending(p => p.FechaInicio)
                .FirstOrDefaultAsync();

        public async Task<bool> ExisteParametro(string nombre)
            => await _contexto.ParametrosPlanilla
                .AnyAsync(p => p.NombreParametro == nombre);

        public async Task<bool> ExisteVersionConFechaInicio(string nombre, DateTime fechaInicio)
        {
            var f = fechaInicio.Date;

            // Sin filtrar por Estado: UQ_ParametroPlanilla_Nombre_Vigencia tampoco lo hace,
            // así que una versión anulada con esa fecha igual haría reventar el INSERT.
            return await _contexto.ParametrosPlanilla
                .AnyAsync(p => p.NombreParametro == nombre && p.FechaInicio == f);
        }

        // ── Escrituras ────────────────────────────────────────────────────────

        public async Task Crear(ParametroPlanillaAD parametro, ContextoAuditoria auditoria)
        {
            // Patrón atómico de FacturacionRepositorio.CambiarActiva: INSERT + bitácora en
            // UN SaveChangesAsync.
            _contexto.ParametrosPlanilla.Add(parametro);

            // IdRegistroAfectado = null: el Id es IDENTITY, no existe hasta el SaveChanges.
            // El parámetro queda identificado por NombreParametro dentro de ValorNuevo.
            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                Tabla,
                null,
                auditoria,
                new { },
                new
                {
                    parametro.NombreParametro,
                    parametro.Valor,
                    parametro.FechaInicio,
                    parametro.FechaFin,
                    parametro.Estado
                }));

            await _contexto.SaveChangesAsync();
        }

        public async Task CrearNuevaVersion(ParametroPlanillaAD nueva, int? idVersionAnterior,
                                            DateTime? fechaFinAnterior, string motivo,
                                            ContextoAuditoria auditoria)
        {
            object valoresAnteriores = new { };

            if (idVersionAnterior.HasValue)
            {
                var anterior = await _contexto.ParametrosPlanilla
                    .FirstOrDefaultAsync(p => p.IdParametroPlanilla == idVersionAnterior.Value);

                if (anterior == null)
                    throw new Exception($"No se encontró la versión con ID {idVersionAnterior.Value}.");

                valoresAnteriores = new
                {
                    anterior.IdParametroPlanilla,
                    anterior.Valor,
                    anterior.FechaInicio,
                    anterior.FechaFin,
                    anterior.Estado
                };

                // Se CIERRA la versión anterior, pero se le MANTIENE el Estado = 1: sigue
                // siendo válida para su período. Ponerle Estado = 0 rompería el histórico.
                anterior.FechaFin = fechaFinAnterior;
                _contexto.ParametrosPlanilla.Update(anterior);
            }

            _contexto.ParametrosPlanilla.Add(nueva);

            // IdRegistroAfectado apunta a la versión que se cierra (la nueva todavía no tiene Id).
            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                Tabla,
                idVersionAnterior,
                auditoria,
                valoresAnteriores,
                new
                {
                    nueva.NombreParametro,
                    nueva.Valor,
                    nueva.FechaInicio,
                    nueva.FechaFin,
                    nueva.Estado,
                    CierreVersionAnterior = fechaFinAnterior,
                    Motivo = motivo
                }));

            // Cerrar la vieja + insertar la nueva + bitácora: una sola transacción.
            await _contexto.SaveChangesAsync();
        }

        public async Task AnularVersion(int idVersion, string motivo, ContextoAuditoria auditoria)
        {
            var version = await _contexto.ParametrosPlanilla
                .FirstOrDefaultAsync(p => p.IdParametroPlanilla == idVersion);

            if (version == null)
                throw new Exception($"No se encontró la versión con ID {idVersion}.");

            var valoresAnteriores = new
            {
                version.NombreParametro,
                version.Valor,
                version.FechaInicio,
                version.FechaFin,
                version.Estado
            };

            // Soft-delete: la fila queda, pero deja de resolver como vigente en cualquier fecha.
            version.Estado = false;

            _contexto.ParametrosPlanilla.Update(version);

            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                Tabla,
                version.IdParametroPlanilla,
                auditoria,
                valoresAnteriores,
                new
                {
                    version.NombreParametro,
                    version.Valor,
                    version.FechaInicio,
                    version.FechaFin,
                    Estado = false,
                    Motivo = motivo
                }));

            await _contexto.SaveChangesAsync();
        }
    }
}
