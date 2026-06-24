using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("PlanillaPeriodo")]
	public class PlanillaPeriodoAD
	{
		[Key]
		public int IdPlanillaPeriodo { get; set; }

		public DateTime FechaInicio { get; set; }
		public DateTime FechaFin { get; set; }

		public string TipoPeriodo { get; set; } = string.Empty;
		public string Estado { get; set; } = string.Empty;

		public DateTime FechaCreacion { get; set; }
		public int IdUsuarioCreacion { get; set; }

		public virtual ICollection<PlanillaDetalleFinancieroAD>? Detalles { get; set; }
	}
}