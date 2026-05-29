using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    [Table("Usuario")]
    public class UsuarioAD
    {
        [Key]
        public int IdUsuario { get; set; }

        public int IdEmpleado { get; set; }
        public int IdRol { get; set; }

        [StringLength(100)]
        public string NombreUsuario { get; set; } = string.Empty;

        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        public bool Estado { get; set; } = true;
    }
}
