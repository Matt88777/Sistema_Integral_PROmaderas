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

        Task<List<PlanillaDetalleFinancieroAD>> ObtenerDetallesPorPeriodo(int idPeriodo);
        Task<PlanillaDetalleFinancieroAD> AgregarDetalle(PlanillaDetalleFinancieroAD detalle);
        Task<PlanillaDetalleFinancieroAD?> ObtenerDetallePorId(int idDetalle);
        Task ActualizarDetalle(PlanillaDetalleFinancieroAD detalle);
        Task EliminarDetalle(int idDetalle);

        Task<List<EmpleadoAD>> ObtenerEmpleadosActivos();
    }
}