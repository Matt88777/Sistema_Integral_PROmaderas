using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("PlanillaDetalle")]
	public class PlanillaDetalleFinancieroAD
	{
		[Key]
		public int IdPlanillaDetalle { get; set; }

		public int IdPlanillaPeriodo { get; set; }

		public int IdEmpleado { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal SalarioBase { get; set; }

		[Column(TypeName = "decimal(10,2)")]
		public decimal HorasOrdinarias { get; set; }

		[Column(TypeName = "decimal(10,2)")]
		public decimal HorasExtra { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal MontoHorasExtra { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal SalarioBruto { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal DeduccionCCSS { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal DeduccionRenta { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal DeduccionesInternas { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal TotalDeducciones { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal SalarioNeto { get; set; }

		// PLA-HU-018: resultado de cobertura.
		public bool TienePolizaINSVigente { get; set; }

		[StringLength(300)]
		public string? AdvertenciaPolizaINS { get; set; }

		// PLA-HU-014: columnas existentes en Sprint 4.
		[Column(TypeName = "decimal(10,2)")]
		public decimal DiasIncapacidad { get; set; }

		// La propiedad utiliza un nombre descriptivo en C#,
		// pero se guarda en la columna existente MontoIncapacidad.
		[Column(
			"MontoIncapacidad",
			TypeName = "decimal(18,2)")]
		public decimal MontoDiasIncapacidad { get; set; }

		// Columnas nuevas de desglose.
		[Column(TypeName = "decimal(18,2)")]
		public decimal MontoPagoPatronalIncapacidad { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal RebajoIncapacidad { get; set; }

		[StringLength(300)]
		public string? DetalleIncapacidad { get; set; }

		// PLA-HU-013: columnas existentes en Sprint 4.
		[Column(TypeName = "decimal(10,2)")]
		public decimal DiasVacaciones { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal MontoVacaciones { get; set; }

		[StringLength(300)]
		public string? DetalleVacaciones { get; set; }

		[ForeignKey(nameof(IdPlanillaPeriodo))]
		public virtual PlanillaPeriodoAD? Periodo { get; set; }

		[ForeignKey(nameof(IdEmpleado))]
		public virtual EmpleadoAD? Empleado { get; set; }
	}
}