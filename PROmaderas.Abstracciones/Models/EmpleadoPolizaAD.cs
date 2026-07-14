using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("EmpleadoPoliza")]
	public class EmpleadoPolizaAD
	{
		[Key]
		public int IdEmpleadoPoliza { get; set; }

		public int IdEmpleado { get; set; }

		public int IdPoliza { get; set; }

		[Column(TypeName = "date")]
		[Display(Name = "Fecha de asignación")]
		public DateTime FechaAsignacion { get; set; } =
			DateTime.Today;

		[Column(TypeName = "date")]
		[Display(Name = "Fecha de exclusión")]
		public DateTime? FechaExclusion { get; set; }

		public bool Activa { get; set; } = true;

		[ForeignKey(nameof(IdEmpleado))]
		public virtual EmpleadoAD? Empleado { get; set; }

		[ForeignKey(nameof(IdPoliza))]
		public virtual PolizaINSAD? Poliza { get; set; }
	}
}