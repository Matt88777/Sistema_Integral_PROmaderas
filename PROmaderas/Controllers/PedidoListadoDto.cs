namespace PROmaderas.UI.Controllers
{
    public class PedidoListadoDto
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
		public bool Activa { get; set; }
	}
    // CLASE NUEVA:OC-HU-004:
    public class HistorialItemDto
    {
        public DateTime FechaCambio { get; set; }
        public string EstadoAnterior { get; set; } = string.Empty;
        public string EstadoNuevo { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
        public string UsuarioCambio { get; set; } = string.Empty;
    }
}
