using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IReportesRepositorio
    {
        Task<List<FacturacionAD>> ObtenerFacturas();
        Task<List<ProductoAD>> ObtenerProductos();
        Task<List<PlanillaDetalleFinancieroAD>> ObtenerPlanillaDetalles();
    }
}