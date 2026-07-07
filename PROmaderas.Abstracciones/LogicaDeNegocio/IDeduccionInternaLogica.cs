using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    public interface IDeduccionInternaLogica
    {
        Task<List<DeduccionInternaAD>> ObtenerTodas();
        Task<DeduccionInternaAD?> ObtenerPorId(int id);
        Task<DeduccionInternaAD> Crear(DeduccionInternaAD deduccion);
        Task Actualizar(DeduccionInternaAD deduccion);
        Task Eliminar(int id);

        Task<List<EmpleadoDeduccionAD>> ObtenerAsignacionesPorEmpleado(int idEmpleado);
        Task AsignarAEmpleado(int idEmpleado, int idDeduccion);
        Task DesasignarDeEmpleado(int idEmpleadoDeduccion);
    }
}