using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    [Table("Licencia")]
    public class LicenciaAD
    {
        [Key]
        public int IdLicencia { get; set; }

        public int IdEmpleado { get; set; }

        [Required]
        public string TipoLicencia { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Dias { get; set; }

        public bool ConGoceSalarial { get; set; }

        public string? Observacion { get; set; }

        [ForeignKey("IdEmpleado")]
        public virtual EmpleadoAD? Empleado { get; set; }
    }
}