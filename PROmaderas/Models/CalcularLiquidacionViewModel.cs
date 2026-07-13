using System.ComponentModel.DataAnnotations;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.UI.Models
{
    // PLA-HU-017: el formulario de cálculo de liquidación.
    //
    // OJO con lo que NO está acá: ningún monto de rubro (vacaciones, aguinaldo, preaviso,
    // cesantía) viaja en el formulario. La Lógica los RECALCULA en el servidor desde
    // (IdEmpleado, FechaSalida, MotivoSalida). Del form solo se aceptan OtrosMontos y la
    // observación, que son datos que el contador aporta y el sistema no puede derivar.
    public class CalcularLiquidacionViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un empleado.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un empleado.")]
        [Display(Name = "Empleado")]
        public int IdEmpleado { get; set; }

        [Required(ErrorMessage = "La fecha de salida es requerida.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de salida")]
        public DateTime FechaSalida { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "El motivo de salida es requerido.")]
        [Display(Name = "Motivo de salida")]
        public string MotivoSalida { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "Los otros montos no pueden ser negativos.")]
        [Display(Name = "Otros montos")]
        public decimal OtrosMontos { get; set; }

        [StringLength(500, ErrorMessage = "La observación no puede superar los 500 caracteres.")]
        [Display(Name = "Observación")]
        public string? Observacion { get; set; }

        // El desglose de la preview. Null hasta que el usuario aprieta "Calcular": la vista lo
        // usa para decidir si muestra el resultado y el botón de guardar.
        public LiquidacionCalculoAD? Calculo { get; set; }
    }
}
