using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IEmpleadoRepositorio
    {
        Task<List<EmpleadoAD>> ObtenerTodos();
        Task<EmpleadoAD> Crear(EmpleadoAD empleado);
        Task Actualizar(EmpleadoAD empleado, ContextoAuditoria auditoria);
        Task Eliminar(int id);
        Task CambiarEstado(int id, ContextoAuditoria auditoria);
        Task<List<SalarioHistorialAD>> ObtenerHistorialSalario(int idEmpleado);
    }
}