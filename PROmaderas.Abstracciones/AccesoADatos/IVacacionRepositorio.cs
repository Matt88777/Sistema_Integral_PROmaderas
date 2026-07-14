using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    // PLA-HU-012: vacaciones disfrutadas y saldo.
    public interface IVacacionRepositorio
    {
        // ── Consultas ─────────────────────────────────────────────────────────

        // Historial completo de un empleado, INCLUIDAS las anuladas: la pantalla las
        // muestra atenuadas (son evidencia de auditoría), aunque no sumen al disfrutado.
        Task<List<VacacionAD>> ObtenerPorEmpleado(int idEmpleado);

        Task<VacacionAD?> ObtenerPorId(int idVacacion);

		// PLA-HU-013: vacaciones disfrutadas que coinciden
		// total o parcialmente con un periodo de planilla.
		Task<List<VacacionAD>> ObtenerPorPeriodo(
			int idEmpleado,
			DateTime fechaInicio,
			DateTime fechaFin);

		// SUM(Dias) de un solo empleado, contando solo las 'Disfrutada'.
		Task<decimal> ObtenerDiasDisfrutados(int idEmpleado);

        Task<EmpleadoAD?> ObtenerEmpleadoPorId(int idEmpleado);

        Task<List<EmpleadoAD>> ObtenerEmpleadosActivos();

        // El disfrutado de TODOS los empleados en UN solo query (GroupBy), para el listado.
        // Misma regla que ObtenerVigentes en ParametroPlanilla: nada de pegarle a la BD
        // una vez por empleado dentro de un for.
        Task<Dictionary<int, decimal>> ObtenerDisfrutadosPorEmpleado();

        // ── Escrituras (las 2 se auditan) ─────────────────────────────────────

        Task Crear(VacacionAD vacacion, ContextoAuditoria auditoria);

        // No hay edición: una vacación mal digitada se anula (con motivo) y se re-registra.
        Task Anular(int idVacacion, string motivo, ContextoAuditoria auditoria);
    }
}
