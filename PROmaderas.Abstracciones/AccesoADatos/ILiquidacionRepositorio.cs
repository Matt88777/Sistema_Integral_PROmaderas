using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    // PLA-HU-017: liquidación de empleados.
    public interface ILiquidacionRepositorio
    {
        // ── Consultas ─────────────────────────────────────────────────────────

        // Todas, INCLUIDAS las anuladas: el listado las muestra atenuadas (son evidencia).
        Task<List<LiquidacionAD>> ObtenerTodas();

        Task<LiquidacionAD?> ObtenerPorId(int id);

        Task<List<LiquidacionAD>> ObtenerPorEmpleado(int idEmpleado);

        // Una liquidación ACTIVA (no anulada) significa que la persona ya fue liquidada.
        // No se liquida dos veces a nadie.
        Task<bool> TieneLiquidacionActiva(int idEmpleado);

        // Solo los ACTIVOS: son los únicos liquidables (a quien ya salió no se le liquida otra vez).
        Task<List<EmpleadoAD>> ObtenerEmpleadosActivos();

        Task<EmpleadoAD?> ObtenerEmpleadoPorId(int idEmpleado);

        // Id -> nombre completo de TODOS los empleados, activos e inactivos, en UN solo query.
        // El listado lo necesita así: al liquidar a alguien queda INACTIVO, de modo que resolver
        // los nombres contra la lista de activos dejaría sin nombre justamente a las filas que
        // el listado muestra. Y nada de una consulta por fila.
        Task<Dictionary<int, string>> ObtenerNombresEmpleados();

        // Liquidacion.IdUsuarioRegistro es NOT NULL con FK a dbo.Usuario (int), pero Identity
        // maneja GUIDs. Mismo puente que usa Factura.IdUsuarioEmisor: se resuelve por correo.
        // Devuelve null si no lo encuentra; el fallback a admin lo aplica la Lógica.
        Task<int?> ObtenerIdUsuarioPorCorreo(string? correo);

        // ── Escrituras (las 2 se auditan) ─────────────────────────────────────

        // ATÓMICO: INSERT de la liquidación + cierre del empleado (FechaSalida, MotivoSalida,
        // Estado = false) + bitácora, todo en UN SaveChangesAsync.
        Task Guardar(LiquidacionAD liquidacion, ContextoAuditoria auditoria);

        // ATÓMICO: soft-delete de la liquidación (Activa = false, Estado = 'Anulada') +
        // REAPERTURA del empleado (Estado = true, FechaSalida y MotivoSalida a null) + bitácora.
        // Si la liquidación se anuló, el despido no ocurrió.
        Task Anular(int idLiquidacion, string motivo, ContextoAuditoria auditoria);
    }
}
