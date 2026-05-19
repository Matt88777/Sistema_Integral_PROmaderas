using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("Categoria")]
	public class CategoriaAD
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "El nombre es requerido")]
		[StringLength(100)]
		public string Nombre { get; set; } = string.Empty;

		[Display(Name = "Activa")]
		public bool Activo { get; set; } = true;

		// Navegación
		public virtual ICollection<ProductoAD>? Productos { get; set; }
	}
}