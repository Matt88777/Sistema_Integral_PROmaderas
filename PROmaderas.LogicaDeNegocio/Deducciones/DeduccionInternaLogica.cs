using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Deducciones
{
    public class DeduccionInternaLogica : IDeduccionInternaLogica
    {
        private readonly IDeduccionInternaRepositorio _repo;

        public DeduccionInternaLogica(IDeduccionInternaRepositorio repo)
        {
            _repo = repo;
        }

        public Task<List<DeduccionInternaAD>> ObtenerTodas() => _repo.ObtenerTodas();
        public Task<DeduccionInternaAD?> ObtenerPorId(int id) => _repo.ObtenerPorId(id);

        public Task<DeduccionInternaAD> Crear(DeduccionInternaAD deduccion)
        {
            if (string.IsNullOrWhiteSpace(deduccion.Nombre))
                throw new ArgumentException("El nombre es requerido.");

            if (deduccion.EsPorcentaje && (deduccion.Porcentaje == null || deduccion.Porcentaje <= 0))
                throw new ArgumentException("Ingrese un porcentaje válido.");

            if (!deduccion.EsPorcentaje && (deduccion.Monto == null || deduccion.Monto <= 0))
                throw new ArgumentException("Ingrese un monto válido.");

            return _repo.Crear(deduccion);
        }

        public Task Actualizar(DeduccionInternaAD deduccion) => _repo.Actualizar(deduccion);
        public Task Eliminar(int id) => _repo.Eliminar(id);

        public Task<List<EmpleadoDeduccionAD>> ObtenerAsignacionesPorEmpleado(int idEmpleado)
            => _repo.ObtenerAsignacionesPorEmpleado(idEmpleado);

        public Task AsignarAEmpleado(int idEmpleado, int idDeduccion)
            => _repo.AsignarAEmpleado(idEmpleado, idDeduccion);

        public Task DesasignarDeEmpleado(int idEmpleadoDeduccion)
            => _repo.DesasignarDeEmpleado(idEmpleadoDeduccion);
    }
}