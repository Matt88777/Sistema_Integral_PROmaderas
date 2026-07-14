using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("Incapacidad")]
	public class IncapacidadAD
	{
		[Key]
		public int IdIncapacidad { get; set; }

		[Required]
		public int IdEmpleado { get; set; }

		[Required]
		[StringLength(100)]
		[Display(Name = "Tipo de incapacidad")]
		public string TipoIncapacidad { get; set; } =
			string.Empty;

		[Required]
		[DataType(DataType.Date)]
		[Column(TypeName = "date")]
		[Display(Name = "Fecha de inicio")]
		public DateTime FechaInicio { get; set; }

		[Required]
		[DataType(DataType.Date)]
		[Column(TypeName = "date")]
		[Display(Name = "Fecha de finalización")]
		public DateTime FechaFin { get; set; }

		// Esta columna ya existe en la base de Sprint 4.
		[Column(TypeName = "decimal(10,2)")]
		[Range(
			0.01,
			99999999.99,
			ErrorMessage = "La cantidad de días debe ser mayor a cero.")]
		[Display(Name = "Cantidad de días")]
		public decimal Dias { get; set; }

		[Required]
		[StringLength(100)]
		[Display(Name = "Número de certificado")]
		public string NumeroCertificado { get; set; } =
			string.Empty;

		[Required]
		[StringLength(50)]
		[Display(Name = "Entidad emisora")]
		public string EntidadEmisora { get; set; } =
			"CCSS";

		[StringLength(250)]
		[Display(Name = "Observaciones")]
		public string? Observacion { get; set; }

		public bool Activa { get; set; } = true;

		public DateTime FechaRegistro { get; set; } =
			DateTime.Now;

		[ForeignKey(nameof(IdEmpleado))]
		public virtual EmpleadoAD? Empleado { get; set; }

		[NotMapped]
		public decimal CantidadDias => Dias;
	}
}