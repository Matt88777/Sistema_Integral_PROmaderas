using System.ComponentModel.DataAnnotations;

namespace PROmaderas.Abstracciones.Models
{
	public class TipoTarimaCrearDTO
	{
		[Required(ErrorMessage = "El código es requerido")]
		[StringLength(50, ErrorMessage = "El código no puede superar los 50 caracteres")]
		[Display(Name = "Código")]
		public string Codigo { get; set; } = string.Empty;

		[Required(ErrorMessage = "El nombre es requerido")]
		[StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
		[Display(Name = "Nombre")]
		public string Nombre { get; set; } = string.Empty;

		[Required(ErrorMessage = "La medida es requerida")]
		[StringLength(100, ErrorMessage = "La medida no puede superar los 100 caracteres")]
		[Display(Name = "Medida")]
		public string Medida { get; set; } = string.Empty;

		[StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres")]
		[Display(Name = "Descripción")]
		public string? Descripcion { get; set; }

		[Required(ErrorMessage = "El precio unitario es requerido")]
		[Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0")]
		[Display(Name = "Precio unitario")]
		public decimal PrecioUnitario { get; set; }

		[Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
		[Display(Name = "Stock mínimo")]
		public int StockMinimo { get; set; }

		[Display(Name = "Activo")]
		public bool Activo { get; set; } = true;
	}
}