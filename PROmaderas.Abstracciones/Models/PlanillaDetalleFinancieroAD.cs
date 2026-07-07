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
        public decimal HorasOrdinarias { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HorasExtra { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalarioBase { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoHorasExtra { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalarioBruto { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DeduccionCCSS { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DeduccionRenta { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeducciones { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalarioNeto { get; set; }

        [ForeignKey("IdEmpleado")]
        public virtual EmpleadoAD? Empleado { get; set; }

        [ForeignKey("IdPlanillaPeriodo")]
        public virtual PlanillaPeriodoAD? Periodo { get; set; }
    }
}