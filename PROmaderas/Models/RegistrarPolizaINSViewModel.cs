using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
	public class RegistrarPolizaINSViewModel
	{
		[Required(ErrorMessage = "Debe seleccionar un empleado.")]
		[Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un empleado válido.")]
		[Display(Name = "Empleado")]
		public int IdEmpleado { get; set; }

		[Required(ErrorMessage = "El número de póliza es obligatorio.")]
		[StringLength(
			100,
			ErrorMessage = "El número de póliza no puede superar los 100 caracteres.")]
		[Display(Name = "Número de póliza")]
		public string NumeroPoliza { get; set; } = string.Empty;

		[Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
		[DataType(DataType.Date)]
		[Display(Name = "Inicio de vigencia")]
		public DateTime FechaInicio { get; set; } = DateTime.Today;

		[Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
		[DataType(DataType.Date)]
		[Display(Name = "Fin de vigencia")]
		public DateTime FechaVencimiento { get; set; } =
			DateTime.Today.AddYears(1);

		[Required(ErrorMessage = "La cobertura es obligatoria.")]
		[StringLength(
			250,
			ErrorMessage = "La cobertura no puede superar los 250 caracteres.")]
		[Display(Name = "Cobertura")]
		public string Cobertura { get; set; } = string.Empty;

		[StringLength(
			500,
			ErrorMessage = "La observación no puede superar los 500 caracteres.")]
		[Display(Name = "Observaciones")]
		public string? Observacion { get; set; }

		public List<SelectListItem> Empleados { get; set; } = new();
	}
}