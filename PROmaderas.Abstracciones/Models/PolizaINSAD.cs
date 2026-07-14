using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("PolizaINS")]
	public class PolizaINSAD
	{
		[Key]
		public int IdPolizaINS { get; set; }

		[Required(ErrorMessage = "Debe seleccionar un empleado.")]
		public int IdEmpleado { get; set; }

		[Required(ErrorMessage = "El número de póliza es obligatorio.")]
		[StringLength(100, ErrorMessage = "El número de póliza no puede superar los 100 caracteres.")]
		[Display(Name = "Número de póliza")]
		public string NumeroPoliza { get; set; } = string.Empty;

		[Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
		[DataType(DataType.Date)]
		[Column(TypeName = "date")]
		[Display(Name = "Inicio de vigencia")]
		public DateTime FechaInicio { get; set; }

		[Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
		[DataType(DataType.Date)]
		[Column(TypeName = "date")]
		[Display(Name = "Fin de vigencia")]
		public DateTime FechaVencimiento { get; set; }

		[Required(ErrorMessage = "La cobertura de la póliza es obligatoria.")]
		[StringLength(250, ErrorMessage = "La cobertura no puede superar los 250 caracteres.")]
		[Display(Name = "Cobertura")]
		public string Cobertura { get; set; } = string.Empty;

		[StringLength(500, ErrorMessage = "La observación no puede superar los 500 caracteres.")]
		[Display(Name = "Observaciones")]
		public string? Observacion { get; set; }

		public bool Activa { get; set; } = true;

		public DateTime FechaRegistro { get; set; } = DateTime.Now;

		[ForeignKey(nameof(IdEmpleado))]
		public virtual EmpleadoAD? Empleado { get; set; }

		[NotMapped]
		public bool EstaVencida => Activa && FechaVencimiento.Date < DateTime.Today;

		[NotMapped]
		public int DiasParaVencer =>
			(FechaVencimiento.Date - DateTime.Today).Days;

		[NotMapped]
		public bool ProximaAVencer =>
			Activa && !EstaVencida && DiasParaVencer <= 30;
	}
}