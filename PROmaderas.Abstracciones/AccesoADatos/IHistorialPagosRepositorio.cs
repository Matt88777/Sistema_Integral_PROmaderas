using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IHistorialPagosRepositorio
    {
        Task<List<PlanillaDetalleFinancieroAD>> ObtenerPorEmpleado(int idEmpleado);
        Task<List<EmpleadoAD>> ObtenerEmpleados();
    }
}