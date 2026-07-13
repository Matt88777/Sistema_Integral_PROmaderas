using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos.Auditoria;

namespace PROmaderas.AccesoADatos.Vacaciones
{
    public class VacacionRepositorio : IVacacionRepositorio
    {
        private const string Tabla = "Vacacion";

        private readonly Contexto _contexto;

        public VacacionRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        // ── Consultas ─────────────────────────────────────────────────────────

        // Trae TODAS, incluidas las anuladas: el historial las muestra atenuadas.
        public async Task<List<VacacionAD>> ObtenerPorEmpleado(int idEmpleado)
            => await _contexto.Vacaciones
                .Where(v => v.IdEmpleado == idEmpleado)
                .OrderByDescending(v => v.FechaInicio)
                .ThenByDescending(v => v.IdVacacion)
                .ToListAsync();

        public async Task<VacacionAD?> ObtenerPorId(int idVacacion)
            => await _contexto.Vacaciones
                .FirstOrDefaultAsync(v => v.IdVacacion == idVacacion);

        // Solo las 'Disfrutada': una anulada no consumió saldo.
        public async Task<decimal> ObtenerDiasDisfrutados(int idEmpleado)
            => await _contexto.Vacaciones
                .Where(v => v.IdEmpleado == idEmpleado && v.Estado == EstadosVacacion.Disfrutada)
                .SumAsync(v => (decimal?)v.Dias) ?? 0m;

        public async Task<EmpleadoAD?> ObtenerEmpleadoPorId(int idEmpleado)
            => await _contexto.Empleados
                .FirstOrDefaultAsync(e => e.IdEmpleado == idEmpleado);

        public async Task<List<EmpleadoAD>> ObtenerEmpleadosActivos()
            => await _contexto.Empleados
                .Where(e => e.Estado == true)
                .OrderBy(e => e.Nombre)
                .ToListAsync();

        public async Task<Dictionary<int, decimal>> ObtenerDisfrutadosPorEmpleado()
        {
            // UN SOLO query con GroupBy para TODOS los empleados. El listado no puede darse el
            // lujo de pegarle a la BD una vez por empleado (misma regla que ObtenerVigentes en
            // ParametroPlanilla). Los empleados sin vacaciones simplemente no vienen en el
            // diccionario: quien lo consume resuelve la ausencia como 0.
            var totales = await _contexto.Vacaciones
                .Where(v => v.Estado == EstadosVacacion.Disfrutada)
                .GroupBy(v => v.IdEmpleado)
                .Select(g => new { IdEmpleado = g.Key, Dias = g.Sum(v => v.Dias) })
                .ToListAsync();

            return totales.ToDictionary(x => x.IdEmpleado, x => x.Dias);
        }

        // ── Escrituras ────────────────────────────────────────────────────────

        public async Task Crear(VacacionAD vacacion, ContextoAuditoria auditoria)
        {
            // Patrón atómico de FacturacionRepositorio.CambiarActiva: INSERT + bitácora en
            // UN SaveChangesAsync.
            _contexto.Vacaciones.Add(vacacion);

            // IdRegistroAfectado = null: IdVacacion es IDENTITY, no existe hasta el SaveChanges.
            // La fila queda identificada por IdEmpleado + fechas dentro de ValorNuevo.
            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                Tabla,
                null,
                auditoria,
                new { },
                new
                {
                    vacacion.IdEmpleado,
                    vacacion.FechaInicio,
                    vacacion.FechaFin,
                    vacacion.Dias,
                    vacacion.Estado,
                    vacacion.Observacion
                }));

            await _contexto.SaveChangesAsync();
        }

        public async Task Anular(int idVacacion, string motivo, ContextoAuditoria auditoria)
        {
            var vacacion = await _contexto.Vacaciones
                .FirstOrDefaultAsync(v => v.IdVacacion == idVacacion);

            if (vacacion == null)
                throw new Exception($"No se encontró la vacación con ID {idVacacion}.");

            // Se capturan ANTES de mutar: el estado y los días previos son la evidencia de
            // cuánto saldo devuelve esta anulación.
            var valoresAnteriores = new
            {
                vacacion.IdEmpleado,
                vacacion.FechaInicio,
                vacacion.FechaFin,
                vacacion.Dias,
                vacacion.Estado,
                vacacion.Observacion
            };

            // Soft-delete: la fila queda, pero deja de sumar a las disfrutadas (todas las
            // consultas filtran por Estado == Disfrutada), así que el saldo se libera solo.
            vacacion.Estado = EstadosVacacion.Anulada;

            _contexto.Vacaciones.Update(vacacion);

            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                Tabla,
                vacacion.IdVacacion,
                auditoria,
                valoresAnteriores,
                new
                {
                    vacacion.IdEmpleado,
                    vacacion.FechaInicio,
                    vacacion.FechaFin,
                    vacacion.Dias,
                    Estado = EstadosVacacion.Anulada,
                    Motivo = motivo
                }));

            // Anulación + bitácora: una sola transacción.
            await _contexto.SaveChangesAsync();
        }
    }
}
