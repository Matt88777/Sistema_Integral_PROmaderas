using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    public interface IEmpleadoLogica
    {
        Task<List<EmpleadoAD>> ObtenerTodos();
        Task<EmpleadoAD> Crear(EmpleadoAD empleado);
        Task Actualizar(EmpleadoAD empleado, ContextoAuditoria auditoria);
        Task Eliminar(int id);
		Task CambiarEstado(int id, ContextoAuditoria auditoria);
	}
}
