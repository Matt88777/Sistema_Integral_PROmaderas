using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    public class PlanillaDetalleFormVM
    {
        [Required]
        public int IdPlanillaPeriodo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un empleado.")]
        [Display(Name = "Empleado")]
        public int IdEmpleado { get; set; }

        [Required(ErrorMessage = "El salario mensual de referencia es obligatorio.")]
        [Range(0.01, 100000000, ErrorMessage = "El salario debe ser mayor a 0.")]
        [Display(Name = "Salario mensual de referencia")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalarioMensual { get; set; }

        [Required(ErrorMessage = "Debe indicar las horas ordinarias trabajadas.")]
        [Range(0, 744, ErrorMessage = "Las horas ordinarias deben estar entre 0 y 744.")]
        [Display(Name = "Horas ordinarias trabajadas")]
        public decimal HorasOrdinarias { get; set; } = 240;

        [Range(0, 200, ErrorMessage = "Las horas extra deben estar entre 0 y 200.")]
        [Display(Name = "Horas extraordinarias trabajadas")]
        public decimal HorasExtra { get; set; } = 0;

        public List<EmpleadoAD>? EmpleadosDisponibles { get; set; }
    }
}