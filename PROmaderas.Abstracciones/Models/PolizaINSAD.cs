using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("PolizaINS")]
	public class PolizaINSAD
	{
		[Key]
		public int IdPoliza { get; set; }

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

		[Column(TypeName = "date")]
		[DataType(DataType.Date)]
		[Display(Name = "Inicio de vigencia")]
		public DateTime FechaInicio { get; set; }

		[Column(TypeName = "date")]
		[DataType(DataType.Date)]
		[Display(Name = "Fin de vigencia")]
		public DateTime FechaVencimiento { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		[Display(Name = "Monto asegurado")]
		public decimal? MontoAsegurado { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal? Prima { get; set; }

		public bool Estado { get; set; } = true;

		[StringLength(250)]
		[Display(Name = "Observaciones")]
		public string? Observacion { get; set; }

		public DateTime FechaCreacion { get; set; } =
			DateTime.Now;

		public virtual ICollection<EmpleadoPolizaAD>
			EmpleadosAsignados
		{ get; set; } =
				new List<EmpleadoPolizaAD>();

		[NotMapped]
		public bool EstaVencida =>
			Estado &&
			FechaVencimiento.Date < DateTime.Today;

		[NotMapped]
		public int DiasParaVencer =>
			(FechaVencimiento.Date - DateTime.Today).Days;

		[NotMapped]
		public bool ProximaAVencer =>
			Estado &&
			!EstaVencida &&
			DiasParaVencer <= 30;
	}
}