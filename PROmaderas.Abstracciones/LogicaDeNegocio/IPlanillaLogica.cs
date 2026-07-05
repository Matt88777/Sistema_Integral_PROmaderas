using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    public interface IPlanillaLogica
    {
        Task<List<PlanillaPeriodoAD>> ObtenerPeriodos();
        Task<PlanillaPeriodoAD?> ObtenerPeriodoPorId(int id);
        Task<PlanillaPeriodoAD> CrearPeriodo(PlanillaPeriodoAD periodo);
        Task CambiarEstadoPeriodo(int id, string nuevoEstado, ContextoAuditoria auditoria);
        Task EliminarPeriodo(int id);

        Task<PlanillaDetalleFinancieroAD> RegistrarHoras(PlanillaDetalleFormVM vm, ContextoAuditoria auditoria);
        Task<PlanillaDetalleFinancieroAD> ActualizarHoras(int idPlanillaDetalle, decimal salarioMensual, 
            decimal horasOrdinarias, decimal horasExtra, ContextoAuditoria auditoria);
        Task EliminarDetalle(int idDetalle);

        Task<List<EmpleadoAD>> ObtenerEmpleadosActivos();
    }
}