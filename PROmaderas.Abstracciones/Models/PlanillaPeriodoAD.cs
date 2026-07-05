using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    [Table("PlanillaPeriodo")]
    public class PlanillaPeriodoAD
    {
        [Key]
        public int IdPlanillaPeriodo { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de inicio")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de fin")]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "El tipo de período es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Tipo de período")]
        public string TipoPeriodo { get; set; } = "Quincenal";

        [StringLength(50)]
        public string Estado { get; set; } = "Borrador";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public int IdUsuarioCreacion { get; set; }

        public virtual ICollection<PlanillaDetalleFinancieroAD>? Detalles { get; set; }

        [NotMapped]
        public int CantidadEmpleados => Detalles?.Count ?? 0;

        [NotMapped]
        public decimal TotalSalarioBruto => Detalles?.Sum(d => d.SalarioBruto) ?? 0m;

        [NotMapped]
        public decimal TotalSalarioNeto => Detalles?.Sum(d => d.SalarioNeto) ?? 0m;
    }
}