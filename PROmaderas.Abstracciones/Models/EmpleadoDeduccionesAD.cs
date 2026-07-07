using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    [Table("EmpleadoDeduccion")]
    public class EmpleadoDeduccionAD
    {
        [Key]
        public int IdEmpleadoDeduccion { get; set; }

        public int IdEmpleado { get; set; }
        public int IdDeduccion { get; set; }

        public EmpleadoAD? Empleado { get; set; }
        public DeduccionInternaAD? Deduccion { get; set; }
    }
}