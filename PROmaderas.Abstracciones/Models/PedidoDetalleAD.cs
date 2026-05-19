using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("PedidoDetalle")]
	public class PedidoDetalleAD
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int PedidoId { get; set; }

		[Required]
		public int ProductoId { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
		public int Cantidad { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal PrecioUnit { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Descuento { get; set; } = 0;

		[Required]
		[Column(TypeName = "decimal(5,2)")]
		public decimal ImpuestoPorc { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal TotalLinea { get; set; }

		// Navegación
		[ForeignKey("PedidoId")]
		public virtual PedidoAD? Pedido { get; set; }

		[ForeignKey("ProductoId")]
		public virtual ProductoAD? Producto { get; set; }
	}
}