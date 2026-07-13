using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
    // PLA-HU-012: registrar un período de vacaciones disfrutado.
    public class RegistrarVacacionViewModel
    {
        public int IdEmpleado { get; set; }

        // Contexto de la pantalla: se rellena desde la BD en cada GET/POST, no viaja de vuelta
        // como dato de confianza. Quien decide de verdad si el saldo alcanza es VacacionLogica.
        public string NombreEmpleado { get; set; } = string.Empty;
        public decimal SaldoDisponible { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de fin es requerida.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de fin")]
        public DateTime FechaFin { get; set; } = DateTime.Today;

        // Se DIGITA: la pantalla sugiere los días naturales del rango, pero el valor que se
        // guarda es este. Admite medios días (0.5).
        [Required(ErrorMessage = "Los días son requeridos.")]
        [Range(0.01, 999, ErrorMessage = "Los días deben ser mayores a cero.")]
        [Display(Name = "Días a rebajar")]
        public decimal Dias { get; set; }

        [StringLength(250, ErrorMessage = "La observación no puede superar los 250 caracteres.")]
        [Display(Name = "Observación")]
        public string? Observacion { get; set; }
    }
}
