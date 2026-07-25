using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IDeduccionInternaRepositorio
    {
        Task<List<DeduccionInternaAD>> ObtenerTodas();
        Task<DeduccionInternaAD?> ObtenerPorId(int id);
        Task<DeduccionInternaAD> Crear(DeduccionInternaAD deduccion);
        Task Actualizar(DeduccionInternaAD deduccion);
        Task Eliminar(int id);

        Task<List<EmpleadoDeduccionAD>> ObtenerAsignacionesPorEmpleado(int idEmpleado);
        Task AsignarAEmpleado(int idEmpleado, int idDeduccion, int? numeroCuotas, decimal? montoTotal);
        Task DesasignarDeEmpleado(int idEmpleadoDeduccion);
        Task<List<EmpleadoDeduccionAD>> ObtenerDeduccionesActivasDeEmpleado(int idEmpleado);
    }
}