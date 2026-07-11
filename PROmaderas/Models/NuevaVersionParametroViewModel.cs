using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
    // PLA-HU-019: nueva versión de un parámetro existente. No edita la versión vieja:
    // la cierra con FechaFin y crea una fila nueva, para no tocar el histórico.
    public class NuevaVersionParametroViewModel
    {
        [Required]
        public string NombreParametro { get; set; } = string.Empty;

        // Contexto (solo lectura en la vista): qué rige hoy. Null si no hay versión vigente.
        public decimal? ValorActual { get; set; }
        public DateTime? VigenteDesde { get; set; }
        public bool TieneVigente => ValorActual.HasValue;

        [Required(ErrorMessage = "El nuevo valor es requerido.")]
        [Range(0, double.MaxValue, ErrorMessage = "El valor no puede ser negativo.")]
        [Display(Name = "Nuevo valor")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "La fecha de inicio de vigencia es requerida.")]
        [DataType(DataType.Date)]
        [Display(Name = "Vigente desde")]
        public DateTime FechaInicio { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Debe indicar el motivo del cambio.")]
        [StringLength(500, MinimumLength = 5,
            ErrorMessage = "El motivo debe tener entre 5 y 500 caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }
}
