using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
	public class RegistrarIncapacidadViewModel
	{
		[Required(ErrorMessage = "Debe seleccionar un empleado.")]
		[Range(
			1,
			int.MaxValue,
			ErrorMessage = "Debe seleccionar un empleado válido.")]
		[Display(Name = "Empleado")]
		public int IdEmpleado { get; set; }

		[Required(ErrorMessage = "El tipo de incapacidad es obligatorio.")]
		[StringLength(50)]
		[Display(Name = "Tipo de incapacidad")]
		public string TipoIncapacidad { get; set; } =
			string.Empty;

		[Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
		[DataType(DataType.Date)]
		[Display(Name = "Fecha de inicio")]
		public DateTime FechaInicio { get; set; } =
			DateTime.Today;

		[Required(ErrorMessage = "La fecha de finalización es obligatoria.")]
		[DataType(DataType.Date)]
		[Display(Name = "Fecha de finalización")]
		public DateTime FechaFin { get; set; } =
			DateTime.Today;

		[Required(ErrorMessage = "El número de certificado es obligatorio.")]
		[StringLength(
			100,
			ErrorMessage = "El certificado no puede superar los 100 caracteres.")]
		[Display(Name = "Número de certificado")]
		public string NumeroCertificado { get; set; } =
			string.Empty;

		[Required(ErrorMessage = "La entidad emisora es obligatoria.")]
		[StringLength(50)]
		[Display(Name = "Entidad emisora")]
		public string EntidadEmisora { get; set; } =
			string.Empty;

		[StringLength(
			500,
			ErrorMessage = "La observación no puede superar los 500 caracteres.")]
		[Display(Name = "Observaciones")]
		public string? Observacion { get; set; }

		public List<SelectListItem> Empleados { get; set; } =
			new();

		public List<SelectListItem> TiposIncapacidad { get; set; } =
			new();

		public List<SelectListItem> EntidadesEmisoras { get; set; } =
			new();
	}
}