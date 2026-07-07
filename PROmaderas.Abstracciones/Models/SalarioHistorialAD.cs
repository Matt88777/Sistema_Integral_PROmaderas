using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    [Table("SalarioHistorial")]
    public class SalarioHistorialAD
    {
        [Key]
        public int IdHistorial { get; set; }
        public int IdEmpleado { get; set; }
        public decimal? SalarioBase { get; set; }
        public string? TipoPago { get; set; }
        public string? JornadaLaboral { get; set; }
        public DateTime FechaCambio { get; set; }
        public string? UsuarioResponsable { get; set; }

        public EmpleadoAD? Empleado { get; set; }
    }
}