using PROmaderas.Abstracciones.Models;

namespace PROmaderas.UI.Models
{
    
    public class ClienteHistorialViewModel
    {
        public ClienteAD Cliente { get; set; } = null!;

        // Lista de pedidos con sus detalles
        public List<PedidoHistorialItemViewModel> Pedidos { get; set; } = new();

        // Totales generales
        public decimal TotalCompras => Pedidos.Sum(p => p.Total);
        public int TotalOrdenes => Pedidos.Count;

        // Sin historial
        public bool TieneHistorial => Pedidos.Any();
    }

    public class PedidoHistorialItemViewModel
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string? Observacion { get; set; }

        // Detalle de líneas de la orden
        public List<PedidoDetalleAD> Detalles { get; set; } = new();

        // Factura asociada
        public FacturacionAD? Factura { get; set; }
    }
}