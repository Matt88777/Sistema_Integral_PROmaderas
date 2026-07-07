using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    [Table("DeduccionInterna")]
    public class DeduccionInternaAD
    {
        [Key]
        public int IdDeduccion { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Monto { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Porcentaje { get; set; }

        public bool EsPorcentaje { get; set; }
        public bool Activa { get; set; } = true;
    }
}