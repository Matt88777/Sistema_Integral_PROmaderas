using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    public interface IParametroPlanillaLogica
    {
        // ── Consumo del cálculo (PLA-HU-012/013/014/017 y el recableado de PlanillaLogica) ──
        Task<decimal?> ObtenerValorVigente(string nombre, DateTime fecha);
        Task<Dictionary<string, decimal>> ObtenerVigentes(DateTime fecha);

        // ── Pantallas ─────────────────────────────────────────────────────────
        // Index: cada parámetro con la versión que rige a esa fecha.
        Task<List<ParametroConVigenciaAD>> ObtenerListado(DateTime fecha);

        // Historial: todas las versiones de un parámetro, más nueva primero.
        Task<List<ParametroPlanillaAD>> ObtenerHistorial(string nombre);

        // Contexto de la pantalla "Nueva versión": qué rige hoy.
        Task<ParametroPlanillaAD?> ObtenerVersionVigente(string nombre, DateTime fecha);

        // ── Escrituras (las 3 se auditan) ─────────────────────────────────────

        // PLA-HU-019: alta de un parámetro que todavía no existe.
        Task Crear(string nombre, decimal valor, DateTime fechaInicio, ContextoAuditoria auditoria);

        // PLA-HU-019: nueva versión de un parámetro existente. Cierra la anterior con
        // FechaFin = fechaInicio - 1 día, sin tocarle el Estado. Motivo obligatorio.
        Task CrearNuevaVersion(string nombre, decimal valor, DateTime fechaInicio,
                               string motivo, ContextoAuditoria auditoria);

        // PLA-HU-019: anula una versión mal digitada (Estado = 0). Motivo obligatorio.
        Task AnularVersion(int idVersion, string motivo, ContextoAuditoria auditoria);
    }
}
