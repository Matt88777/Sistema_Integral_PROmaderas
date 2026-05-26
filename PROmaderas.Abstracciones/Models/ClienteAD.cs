using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    [Table("Cliente")]
    public class ClienteAD
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(150)]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula jurídica es requerida")]
        [StringLength(20)]
        [RegularExpression(@"^\d{6,20}$", ErrorMessage = "La cédula debe contener sólo dígitos (6-20 caracteres)")]
        [Display(Name = "Cédula Jurídica")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido")]
        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = string.Empty;

        [StringLength(20)]
        [RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Teléfono inválido")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(300)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "La condición de pago es requerida")]
        [StringLength(100)]
        [Display(Name = "Condición de Pago")]
        public string CondicionPago { get; set; } = "Contado";

        [Display(Name = "¿Exonerado?")]
        public bool Exonerado { get; set; } = false;

        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Porcentaje de Exoneración (%)")]
        public decimal PorcentajeExoneracion { get; set; } = 0;

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? UsuarioIdentityId { get; set; }

        public virtual ICollection<PedidoAD>? Pedidos { get; set; }
    }
}