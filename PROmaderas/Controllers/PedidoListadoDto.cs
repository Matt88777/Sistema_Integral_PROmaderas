namespace PROmaderas.UI.Controllers
{
	internal class PedidoListadoDto
	{
		public int Id { get; set; }
		public DateTime Fecha { get; set; }
		public string Cliente { get; set; } 
		public string Usuario { get; set; }
		public decimal Subtotal { get; set; }
		public decimal Impuestos { get; set; }
		public decimal Total { get; set; }
		public string Estado { get; set; }
	}
}