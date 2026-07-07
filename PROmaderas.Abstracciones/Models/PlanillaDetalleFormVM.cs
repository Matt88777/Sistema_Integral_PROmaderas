using System.ComponentModel.DataAnnotations;

namespace PROmaderas.Abstracciones.Models
{
    public class PlanillaDetalleFormVM
    {
        public int IdPlanillaPeriodo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un empleado.")]
        public int IdEmpleado { get; set; }

        [Required(ErrorMessage = "El salario mensual es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El salario mensual debe ser mayor a 0.")]
        public decimal SalarioMensual { get; set; }

        [Required(ErrorMessage = "Las horas ordinarias son requeridas.")]
        [Range(0, double.MaxValue, ErrorMessage = "Las horas ordinarias no pueden ser negativas.")]
        public decimal HorasOrdinarias { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Las horas extra no pueden ser negativas.")]
        public decimal HorasExtra { get; set; }
    }
}