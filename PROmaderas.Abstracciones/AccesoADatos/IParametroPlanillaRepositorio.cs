using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IParametroPlanillaRepositorio
    {
        // ── Consumo del cálculo ───────────────────────────────────────────────
        // Estos dos los va a usar el recableado de PlanillaLogica y las HU 012/013/014/017.
        // Resolución de la versión que rige a una fecha:
        //   NombreParametro == nombre AND Estado == true
        //   AND fecha >= FechaInicio AND (FechaFin == null OR fecha <= FechaFin)

        // Valor del parámetro vigente a esa fecha, o null si no hay versión que la cubra.
        Task<decimal?> ObtenerValorVigente(string nombre, DateTime fecha);

        // TODOS los parámetros vigentes a esa fecha, en UNA sola query (para no pegarle
        // a la BD una vez por parámetro dentro del cálculo de planilla).
        Task<Dictionary<string, decimal>> ObtenerVigentes(DateTime fecha);

        // ── CRUD / versionado ─────────────────────────────────────────────────

        // Index: un renglón por NombreParametro con la versión que rige a esa fecha.
        Task<List<ParametroConVigenciaAD>> ObtenerParametrosConVigente(DateTime fecha);

        // Todas las versiones de un parámetro (incluidas las anuladas), más nueva primero.
        Task<List<ParametroPlanillaAD>> ObtenerHistorial(string nombre);

        // Versión que rige a esa fecha (la entidad, no solo el valor).
        Task<ParametroPlanillaAD?> ObtenerVersionVigente(string nombre, DateTime fecha);

        // Versión puntual por Id (para anular).
        Task<ParametroPlanillaAD?> ObtenerVersionPorId(int idVersion);

        // Versión NO anulada con la FechaInicio más alta: es la que hay que cerrar
        // al crear una versión nueva. Null si el parámetro no tiene versiones válidas.
        Task<ParametroPlanillaAD?> ObtenerUltimaVersionActiva(string nombre);

        // Versión válida INMEDIATAMENTE ANTERIOR a una fecha (la de FechaInicio más alta
        // entre las menores). Es la que hay que reabrir cuando se anula la que la cerró.
        Task<ParametroPlanillaAD?> ObtenerVersionAnteriorActiva(string nombre, DateTime fechaInicio);

        // Versión válida INMEDIATAMENTE POSTERIOR a una fecha (la de FechaInicio más baja
        // entre las mayores). Contra ella se vuelve a cerrar la anterior al anular.
        Task<ParametroPlanillaAD?> ObtenerVersionSiguienteActiva(string nombre, DateTime fechaInicio);

        // ¿Existe el parámetro con ese nombre, en cualquier versión (anuladas incluidas)?
        Task<bool> ExisteParametro(string nombre);

        // ¿Ya hay una versión de ese parámetro con esa misma FechaInicio? Mira TODAS las
        // filas, anuladas incluidas: el UNIQUE de la BD no filtra por Estado.
        Task<bool> ExisteVersionConFechaInicio(string nombre, DateTime fechaInicio);

        // INSERT de la primera versión de un parámetro nuevo + bitácora, atómico.
        Task Crear(ParametroPlanillaAD parametro, ContextoAuditoria auditoria);

        // Cierra la versión anterior con fechaFinAnterior (MANTENIENDO su Estado = 1),
        // inserta la nueva y escribe la bitácora: todo en UN SaveChangesAsync.
        // idVersionAnterior/fechaFinAnterior van en null si el parámetro no tiene ninguna
        // versión válida que cerrar (todas anuladas).
        Task CrearNuevaVersion(ParametroPlanillaAD nueva, int? idVersionAnterior,
                               DateTime? fechaFinAnterior, string motivo, ContextoAuditoria auditoria);

        // Soft-delete de una versión (Estado = 0) + bitácora, atómico.
        // idVersionAnterior/fechaFinAnterior: la versión que esta cerró recupera su cobertura.
        // fechaFinAnterior en null = la anterior vuelve a quedar abierta (vigente indefinida).
        // idVersionAnterior en null = no había ninguna anterior válida que reabrir.
        Task AnularVersion(int idVersion, int? idVersionAnterior, DateTime? fechaFinAnterior,
                           string motivo, ContextoAuditoria auditoria);
    }
}
