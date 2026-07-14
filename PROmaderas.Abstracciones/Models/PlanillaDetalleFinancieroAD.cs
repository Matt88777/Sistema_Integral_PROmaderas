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

		// PLA-HU-018: resultado de la validación de cobertura
		// realizada al calcular la planilla.
		[Display(Name = "Póliza INS vigente")]
		public bool TienePolizaINSVigente { get; set; }

		[StringLength(300)]
		[Display(Name = "Advertencia de póliza INS")]
		public string? AdvertenciaPolizaINS { get; set; }

		[Column(TypeName = "decimal(18,2)")]
        public decimal HorasOrdinarias { get; set; }

        [Column(TypeName = "decimal(18,2)")]
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

        [ForeignKey("IdPlanillaPeriodo")]
        public virtual PlanillaPeriodoAD? Periodo { get; set; }

        public virtual EmpleadoAD? Empleado { get; set; }
    }
}