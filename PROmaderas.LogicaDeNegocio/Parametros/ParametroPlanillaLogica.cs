using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Parametros
{
    public class ParametroPlanillaLogica : IParametroPlanillaLogica
    {
        private readonly IParametroPlanillaRepositorio _repositorio;

        public ParametroPlanillaLogica(IParametroPlanillaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        // ── Consumo del cálculo ───────────────────────────────────────────────

        public async Task<decimal?> ObtenerValorVigente(string nombre, DateTime fecha)
            => await _repositorio.ObtenerValorVigente(LimpiarNombre(nombre), fecha);

        public async Task<Dictionary<string, decimal>> ObtenerVigentes(DateTime fecha)
            => await _repositorio.ObtenerVigentes(fecha);

        // ── Pantallas ─────────────────────────────────────────────────────────

        public async Task<List<ParametroConVigenciaAD>> ObtenerListado(DateTime fecha)
            => await _repositorio.ObtenerParametrosConVigente(fecha);

        public async Task<List<ParametroPlanillaAD>> ObtenerHistorial(string nombre)
            => await _repositorio.ObtenerHistorial(LimpiarNombre(nombre));

        public async Task<ParametroPlanillaAD?> ObtenerVersionVigente(string nombre, DateTime fecha)
            => await _repositorio.ObtenerVersionVigente(LimpiarNombre(nombre), fecha);

        // ── Escrituras ────────────────────────────────────────────────────────

        public async Task Crear(string nombre, decimal valor, DateTime fechaInicio,
                                ContextoAuditoria auditoria)
        {
            var nombreLimpio = ValidarNombre(nombre);
            ValidarValor(valor);

            if (await _repositorio.ExisteParametro(nombreLimpio))
                throw new ArgumentException(
                    $"El parámetro '{nombreLimpio}' ya existe. Para cambiar su valor cree una versión nueva.");

            var parametro = new ParametroPlanillaAD
            {
                NombreParametro = nombreLimpio,
                Valor = valor,
                FechaInicio = fechaInicio.Date,
                FechaFin = null,      // abierta: rige hasta que una versión nueva la cierre
                Estado = true
            };

            await _repositorio.Crear(parametro, auditoria);
        }

        public async Task CrearNuevaVersion(string nombre, decimal valor, DateTime fechaInicio,
                                            string motivo, ContextoAuditoria auditoria)
        {
            var nombreLimpio = ValidarNombre(nombre);
            ValidarValor(valor);
            var motivoLimpio = ValidarMotivo(motivo);
            var desde = fechaInicio.Date;

            if (!await _repositorio.ExisteParametro(nombreLimpio))
                throw new ArgumentException($"El parámetro '{nombreLimpio}' no existe.");

            // Chocaría con UQ_ParametroPlanilla_Nombre_Vigencia (duplicate key). Se valida acá
            // para dar un mensaje entendible en vez de una excepción de SQL Server.
            if (await _repositorio.ExisteVersionConFechaInicio(nombreLimpio, desde))
                throw new ArgumentException(
                    $"Ya existe una versión de '{nombreLimpio}' que arranca el {desde:dd/MM/yyyy}.");

            // La versión a cerrar es la última NO anulada. Puede no haber ninguna (si todas se
            // anularon): en ese caso no hay nada que cerrar y la nueva arranca sola.
            var anterior = await _repositorio.ObtenerUltimaVersionActiva(nombreLimpio);

            if (anterior != null && desde <= anterior.FechaInicio)
                throw new ArgumentException(
                    $"La vigencia debe arrancar después del {anterior.FechaInicio:dd/MM/yyyy}, " +
                    "que es cuando arranca la versión actual. No se pueden insertar versiones en el pasado.");

            // La anterior se cierra el día antes de que arranque la nueva: sin huecos, sin solapes.
            // Su Estado NO se toca: sigue en 1 porque las planillas de su período la necesitan.
            DateTime? fechaFinAnterior = anterior != null ? desde.AddDays(-1) : null;

            var nueva = new ParametroPlanillaAD
            {
                NombreParametro = nombreLimpio,
                Valor = valor,
                FechaInicio = desde,
                FechaFin = null,
                Estado = true
            };

            await _repositorio.CrearNuevaVersion(
                nueva, anterior?.IdParametroPlanilla, fechaFinAnterior, motivoLimpio, auditoria);
        }

        public async Task AnularVersion(int idVersion, string motivo, ContextoAuditoria auditoria)
        {
            var motivoLimpio = ValidarMotivo(motivo);

            var version = await _repositorio.ObtenerVersionPorId(idVersion);
            if (version == null)
                throw new ArgumentException("La versión del parámetro no existe.");

            if (!version.Estado)
                throw new ArgumentException("Esta versión ya está anulada.");

            // Al crearse, esta versión le puso FechaFin a la anterior. Si se anula sin devolverle
            // esa cobertura, queda un rango de fechas sin ninguna versión vigente y las planillas
            // de ese rango revientan con "No hay una versión vigente del parámetro X".
            var anterior = await _repositorio.ObtenerVersionAnteriorActiva(
                version.NombreParametro, version.FechaInicio);

            DateTime? fechaFinAnterior = null;

            if (anterior != null)
            {
                // La anterior NO siempre vuelve a quedar abierta: se cierra contra la siguiente
                // versión que siga siendo válida. Con v1/v2/v3, anular v2 cierra v1 contra v3.
                // Solo si no queda ninguna versión válida posterior, FechaFin vuelve a null.
                var siguiente = await _repositorio.ObtenerVersionSiguienteActiva(
                    version.NombreParametro, version.FechaInicio);

                fechaFinAnterior = siguiente?.FechaInicio.AddDays(-1);
            }

            await _repositorio.AnularVersion(
                idVersion, anterior?.IdParametroPlanilla, fechaFinAnterior, motivoLimpio, auditoria);
        }

        // ── Validaciones ──────────────────────────────────────────────────────

        private static string LimpiarNombre(string nombre) => (nombre ?? string.Empty).Trim();

        private static string ValidarNombre(string nombre)
        {
            var limpio = LimpiarNombre(nombre);

            if (limpio.Length == 0)
                throw new ArgumentException("El nombre del parámetro es requerido.");

            if (limpio.Length > 100)
                throw new ArgumentException("El nombre del parámetro no puede superar los 100 caracteres.");

            // Sin espacios: el nombre es una clave que consume el código, no una etiqueta.
            if (limpio.Any(char.IsWhiteSpace))
                throw new ArgumentException("El nombre del parámetro no puede contener espacios.");

            return limpio;
        }

        private static void ValidarValor(decimal valor)
        {
            if (valor < 0)
                throw new ArgumentException("El valor no puede ser negativo.");
        }

        // El motivo es la justificación del cambio: sin él no se guarda nada en la bitácora.
        private static string ValidarMotivo(string motivo)
        {
            var limpio = (motivo ?? string.Empty).Trim();

            if (limpio.Length == 0)
                throw new ArgumentException("Debe indicar el motivo del cambio.");

            return limpio;
        }
    }
}
