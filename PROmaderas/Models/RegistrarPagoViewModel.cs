using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
    public class RegistrarPagoViewModel
    {
        // Contexto (solo lectura en la vista).
        public int FacturaId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public decimal SaldoPendiente { get; set; }

        // Datos del pago que se postean.
        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "Seleccione una forma de pago.")]
        public string FormaPago { get; set; } = string.Empty;

        public string? Referencia { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.Today;
    }
}
