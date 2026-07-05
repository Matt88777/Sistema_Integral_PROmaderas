using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IPlanillaRepositorio
    {
        Task<List<PlanillaPeriodoAD>> ObtenerPeriodos();
        Task<PlanillaPeriodoAD?> ObtenerPeriodoPorId(int id);
        Task<PlanillaPeriodoAD> CrearPeriodo(PlanillaPeriodoAD periodo);
        Task CambiarEstadoPeriodo(int id, string nuevoEstado, ContextoAuditoria auditoria);
        Task EliminarPeriodo(int id);

        Task<PlanillaDetalleFinancieroAD> AgregarDetalle(PlanillaDetalleFinancieroAD detalle, ContextoAuditoria auditoria);
        Task<PlanillaDetalleFinancieroAD> ActualizarDetalle(int idDetalle, decimal horasOrdinarias, decimal horasExtra,
            decimal salarioBase, decimal montoHorasExtra, decimal salarioBruto, ContextoAuditoria auditoria);
        Task EliminarDetalle(int idDetalle);

        Task<List<EmpleadoAD>> ObtenerEmpleadosActivos();
    }
}