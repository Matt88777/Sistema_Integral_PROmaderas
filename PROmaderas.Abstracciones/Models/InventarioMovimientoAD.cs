using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("InventarioMovimiento")]
	public class InventarioMovimientoAD
	{
		[Key]
		public int IdMovimiento { get; set; }

		public int IdTipoTarima { get; set; }

		public int IdUsuarioRegistro { get; set; }

		[Required]
		[StringLength(30)]
		public string TipoMovimiento { get; set; } = string.Empty;

		[Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
		public int Cantidad { get; set; }

		public DateTime FechaMovimiento { get; set; } = DateTime.Now;

		[StringLength(250)]
		public string? Motivo { get; set; }

		public int? IdProduccion { get; set; }

		public int? IdOrdenCompra { get; set; }

		[ForeignKey("IdTipoTarima")]
		public virtual ProductoAD? Producto { get; set; }

		[ForeignKey("IdOrdenCompra")]
		public virtual PedidoAD? OrdenCompra { get; set; }
	}
}