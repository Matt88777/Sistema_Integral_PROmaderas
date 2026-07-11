using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    // Un parámetro de planilla tiene VARIAS filas: una por versión de vigencia.
    // UQ_ParametroPlanilla_Nombre_Vigencia = UNIQUE (NombreParametro, FechaInicio).
    //
    // FechaInicio/FechaFin dicen CUÁNDO RIGE la versión.
    // Estado dice SI LA VERSIÓN ES VÁLIDA (soft-delete), NO si "ya pasó":
    // una versión cerrada con FechaFin sigue con Estado = 1 porque las planillas
    // de su período la necesitan. Estado = 0 es "se anuló por error, ignorar siempre".
    [Table("ParametroPlanilla")]
    public class ParametroPlanillaAD
    {
        [Key]
        public int IdParametroPlanilla { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreParametro { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,4)")]
        public decimal Valor { get; set; }

        [Column(TypeName = "date")]
        public DateTime FechaInicio { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FechaFin { get; set; }

        public bool Estado { get; set; } = true;
    }
}
