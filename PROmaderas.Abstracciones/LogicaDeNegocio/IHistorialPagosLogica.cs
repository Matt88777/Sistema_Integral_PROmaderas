using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    public interface IHistorialPagosLogica
    {
        Task<List<PlanillaDetalleFinancieroAD>> ObtenerHistorialPorEmpleado(int idEmpleado);
        Task<List<EmpleadoAD>> ObtenerEmpleados();
    }
}