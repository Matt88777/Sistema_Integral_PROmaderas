using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Empleados
{
    public class EmpleadoLogica : IEmpleadoLogica
    {
        private readonly IEmpleadoRepositorio _repositorio;

        public EmpleadoLogica(IEmpleadoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<List<EmpleadoAD>> ObtenerTodos() => _repositorio.ObtenerTodos();

        public Task<EmpleadoAD> Crear(EmpleadoAD empleado) => _repositorio.Crear(empleado);

        public Task Actualizar(EmpleadoAD empleado, ContextoAuditoria auditoria)
            => _repositorio.Actualizar(empleado, auditoria);

		public Task CambiarEstado(int id, ContextoAuditoria auditoria)
            => _repositorio.CambiarEstado(id, auditoria);

		public Task Eliminar(int id) => _repositorio.Eliminar(id);
    }
}
