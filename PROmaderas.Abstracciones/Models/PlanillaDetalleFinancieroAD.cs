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
		public decimal SalarioNeto { get; set; }

		[ForeignKey("IdPlanillaPeriodo")]
		public virtual PlanillaPeriodoAD? Periodo { get; set; }
	}
}