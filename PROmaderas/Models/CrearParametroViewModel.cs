using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
    // PLA-HU-019: alta de un parámetro que todavía no existe.
    public class CrearParametroViewModel
    {
        [Required(ErrorMessage = "El nombre del parámetro es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        [RegularExpression(@"^\S+$", ErrorMessage = "El nombre no puede contener espacios.")]
        [Display(Name = "Nombre del parámetro")]
        public string NombreParametro { get; set; } = string.Empty;

        [Required(ErrorMessage = "El valor es requerido.")]
        [Range(0, double.MaxValue, ErrorMessage = "El valor no puede ser negativo.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "La fecha de inicio de vigencia es requerida.")]
        [DataType(DataType.Date)]
        [Display(Name = "Vigente desde")]
        public DateTime FechaInicio { get; set; } = DateTime.Today;
    }
}
