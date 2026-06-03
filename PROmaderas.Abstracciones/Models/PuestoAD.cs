using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    [Table("Puesto")]
    public class PuestoAD
    {
        [Key]
        public int IdPuesto { get; set; }

        [StringLength(100)]
        public string NombrePuesto { get; set; } = string.Empty;

        public int IdDepartamento { get; set; }

        public bool Estado { get; set; }
    }
}
