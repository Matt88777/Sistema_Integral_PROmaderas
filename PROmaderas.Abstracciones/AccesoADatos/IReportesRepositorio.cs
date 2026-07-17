using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IReportesRepositorio
    {
        Task<List<FacturacionAD>> ObtenerFacturas();
        Task<List<ProductoAD>> ObtenerProductos();
        Task<List<PlanillaDetalleFinancieroAD>> ObtenerPlanillaDetalles();
        Task<List<InventarioExistenciaDTO>> ObtenerExistenciasInventario();
        Task<List<InventarioMovimientoDTO>> ObtenerMovimientosInventario();
        Task<List<VentaPeriodoDTO>> ObtenerVentasPorPeriodo(string tipoPeriodo, DateTime fechaInicio, DateTime fechaFin);

    }
}