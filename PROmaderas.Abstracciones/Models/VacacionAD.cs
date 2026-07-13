using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    // PLA-HU-012: un período de vacaciones DISFRUTADO por un empleado.
    // Lo acumulado no vive acá: se calcula (ver VacacionLogica). Esta tabla solo guarda
    // lo que el empleado ya se tomó.
    //
    // Dias NO se deriva del rango de fechas: se DIGITA (admite medios días, y un rango
    // puede tener feriados o fines de semana que no se cobran). La pantalla sugiere los
    // días naturales del rango, pero manda lo que el usuario escribe.
    //
    // Una vacación no se edita: se ANULA (Estado = 'Anulada') y se vuelve a registrar.
    // Las anuladas dejan de sumar a las disfrutadas, pero la fila queda para la auditoría.
    [Table("Vacacion")]
    public class VacacionAD
    {
        [Key]
        public int IdVacacion { get; set; }

        public int IdEmpleado { get; set; }

        [Column(TypeName = "date")]
        public DateTime FechaInicio { get; set; }

        [Column(TypeName = "date")]
        public DateTime FechaFin { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Dias { get; set; }

        // Siempre uno de EstadosVacacion. La columna no tiene CHECK: ver ese catálogo.
        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Observacion { get; set; }
    }
}
