using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
	public class RegistrarPolizaINSViewModel
	{
		[Required]
		[StringLength(50)]
		[Display(Name = "Número de póliza")]
		public string NumeroPoliza { get; set; } =
			string.Empty;

		[Required]
		[StringLength(100)]
		[Display(Name = "Tipo de póliza")]
		public string TipoPoliza { get; set; } =
			"Riesgos del Trabajo";

		[Required]
		[StringLength(100)]
		public string Aseguradora { get; set; } =
			"INS";

		[Required]
		[DataType(DataType.Date)]
		[Display(Name = "Inicio de vigencia")]
		public DateTime FechaInicio { get; set; } =
			DateTime.Today;

		[Required]
		[DataType(DataType.Date)]
		[Display(Name = "Fin de vigencia")]
		public DateTime FechaVencimiento { get; set; } =
			DateTime.Today.AddYears(1);

		[Range(
			0,
			9999999999999999.99,
			ErrorMessage = "El monto asegurado no puede ser negativo.")]
		[Display(Name = "Monto asegurado")]
		public decimal? MontoAsegurado { get; set; }

		[Range(
			0,
			9999999999999999.99,
			ErrorMessage = "La prima no puede ser negativa.")]
		public decimal? Prima { get; set; }

		[StringLength(250)]
		[Display(Name = "Observaciones")]
		public string? Observacion { get; set; }

		[Display(Name = "Empleados cubiertos")]
		public List<int> IdsEmpleados { get; set; } =
			new();

		public List<SelectListItem> Empleados { get; set; } =
			new();
	}
}