namespace PROmaderas.UI.Models
{
    public class CambiarEstadoFacturaViewModel
    {
        public int FacturaId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;
        // Valor que se postea desde el dropdown.
        public string NuevoEstado { get; set; } = string.Empty;
    }
}
