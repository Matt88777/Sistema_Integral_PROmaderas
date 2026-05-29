using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
    public class CreatePedidoViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [StringLength(250)]
        [Display(Name = "Observaciones")]
        public string? Observacion { get; set; }

        public List<DetallePedidoItem> Detalles { get; set; } = new();
    }

    public class DetallePedidoItem
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnit { get; set; }
    }
}
