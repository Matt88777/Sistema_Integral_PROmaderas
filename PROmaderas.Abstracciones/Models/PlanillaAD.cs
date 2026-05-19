using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("Planilla")]
	public class PlanillaAD
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(150)]
		public string NombreEmpleado { get; set; } = string.Empty;

		[Required]
		[StringLength(20)]
		public string Cedula { get; set; } = string.Empty;

		[Required]
		[StringLength(100)]
		public string Puesto { get; set; } = string.Empty;

		[Required]
		public DateTime FechaIngreso { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal SalarioBase { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal HorasExtra { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Bonificacion { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Deducciones { get; set; }

		[NotMapped]
		public decimal SalarioNeto => (SalarioBase + HorasExtra + Bonificacion) - Deducciones;

		public bool Activo { get; set; } = true;
	}
}