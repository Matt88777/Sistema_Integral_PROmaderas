using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    [Table("OrdenCompra")]
    public class PedidoAD
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroOrden { get; set; } = string.Empty;

        [Required]
        public int ClienteId { get; set; }

        // IdVendedor INT FK -> Usuario
        public int VendedorId { get; set; }

        // UsuarioId STRING (Identity) - ignorado en EF, solo para compatibilidad
        [NotMapped]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(250)]
        public string? Observacion { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Impuestos { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente";

        public bool Activa { get; set; } = true;

        // Navegación
        [ForeignKey("ClienteId")]
        public virtual ClienteAD? Cliente { get; set; }

        public virtual ICollection<PedidoDetalleAD>? Detalles { get; set; }
    }
}
