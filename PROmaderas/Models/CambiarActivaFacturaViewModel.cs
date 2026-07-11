using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
    // FAC-HU-005: confirmación de inactivar / reactivar una factura (solo Administrador).
    public class CambiarActivaFacturaViewModel
    {
        // Contexto (solo lectura en la vista).
        public int FacturaId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;
        public decimal SaldoPendiente { get; set; }

        // Estado actual del flag Activa: decide si la pantalla inactiva o reactiva.
        // En el POST NO se confía en este valor: se relee de la BD.
        public bool ActivaActual { get; set; }

        // Advertencia previa a inactivar: la factura ya tiene pagos registrados.
        public int CantidadPagos { get; set; }
        public decimal TotalPagado { get; set; }
        public bool TienePagos => CantidadPagos > 0;

        // Bloqueo previo a reactivar: la orden ya tiene otra factura activa. Null = se puede.
        // Es solo el aviso de la vista; quien corta de verdad es FacturacionLogica.Reactivar.
        public string? ImpedimentoReactivacion { get; set; }
        public bool PuedeConfirmar => ImpedimentoReactivacion == null;

        // Único dato que se postea. Obligatorio: queda en la bitácora como justificación.
        [Required(ErrorMessage = "Debe indicar el motivo del cambio.")]
        [StringLength(500, MinimumLength = 5,
            ErrorMessage = "El motivo debe tener entre 5 y 500 caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }
}
