using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("Pedido")]
	public class PedidoAD
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int ClienteId { get; set; }

		[Required]
		[StringLength(450)]
		public string UsuarioId { get; set; } = string.Empty;

		[Required]
		public DateTime Fecha { get; set; } = DateTime.Now;

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Subtotal { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Impuestos { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Total { get; set; }

		[Required]
		[StringLength(50)]
		public string Estado { get; set; } = "Pendiente";

		// Navegación
		[ForeignKey("ClienteId")]
		public virtual ClienteAD? Cliente { get; set; }

		public virtual ICollection<PedidoDetalleAD>? Detalles { get; set; }
	}
}