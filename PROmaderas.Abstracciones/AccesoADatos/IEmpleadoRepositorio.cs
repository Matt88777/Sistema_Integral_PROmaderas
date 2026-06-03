using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IEmpleadoRepositorio
    {
        Task<List<EmpleadoAD>> ObtenerTodos();
        Task<EmpleadoAD> Crear(EmpleadoAD empleado);
        Task Actualizar(EmpleadoAD empleado, ContextoAuditoria auditoria);
        Task Eliminar(int id);
    }
}
