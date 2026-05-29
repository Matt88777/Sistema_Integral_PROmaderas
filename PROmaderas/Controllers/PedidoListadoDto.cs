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
    }
}
