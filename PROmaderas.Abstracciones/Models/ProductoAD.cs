using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("Producto")]
	public class ProductoAD
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "El nombre es requerido")]
		[StringLength(150)]
		[Display(Name = "Nombre del Producto")]
		public string Nombre { get; set; } = string.Empty;

		[Required(ErrorMessage = "La categoría es requerida")]
		[Display(Name = "Categoría")]
		public int CategoriaId { get; set; }

		[Required(ErrorMessage = "El precio es requerido")]
		[Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Precio { get; set; }

		[Required(ErrorMessage = "El impuesto es requerido")]
		[Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100")]
		[Column(TypeName = "decimal(5,2)")]
		[Display(Name = "Impuesto %")]
		public decimal ImpuestoPorc { get; set; }

		[Required(ErrorMessage = "El stock es requerido")]
		[Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
		public int Stock { get; set; }

		[Required(ErrorMessage = "La imagen es requerida")]
		[StringLength(300)]
		[Display(Name = "URL de Imagen")]
		public string ImagenUrl { get; set; } = string.Empty;

		[Display(Name = "Activo")]
		public bool Activo { get; set; } = true;

		// Navegación
		[ForeignKey("CategoriaId")]
		public virtual CategoriaAD? Categoria { get; set; }

		public virtual ICollection<PedidoDetalleAD>? PedidoDetalles { get; set; }
	}
}