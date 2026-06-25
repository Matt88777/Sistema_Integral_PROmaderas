using System.ComponentModel.DataAnnotations;

namespace PROmaderas.Abstracciones.Models
{
    /// <summary>
    /// DTO que representa el formulario de registro de producción semanal.
    /// INV-HU-001: Escenarios 1, 2 y 3.
    /// </summary>
    public class ProduccionSemanalDTO
    {
        [Required(ErrorMessage = "El tipo de tarima es requerido")]
        [Display(Name = "Tipo de Tarima")]
        public int IdTipoTarima { get; set; }

        [Required(ErrorMessage = "La cantidad producida es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser un número entero mayor a 0")]
        [Display(Name = "Cantidad Producida")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "La fecha de producción es requerida")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Producción")]
        public DateTime FechaProduccion { get; set; } = DateTime.Today;

        [StringLength(250, ErrorMessage = "El motivo no puede superar los 250 caracteres")]
        [Display(Name = "Observación (opcional)")]
        public string? Motivo { get; set; }
    }
}