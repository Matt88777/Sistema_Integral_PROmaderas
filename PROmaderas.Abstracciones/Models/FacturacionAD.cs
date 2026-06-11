using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    [Table("Factura")]
    public class FacturacionAD
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(30)]
        public string NumeroFactura { get; set; } = string.Empty;
        [Required]
        public int PedidoId { get; set; }
        [Required]
        public int ClienteId { get; set; }
        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Exoneracion { get; set; } = 0;
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Impuestos { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "Emitida";
        public bool Activa { get; set; } = true;
        public string MetodoPago { get; set; } = string.Empty;
        [ForeignKey("PedidoId")]
        public virtual PedidoAD? Pedido { get; set; }
        [ForeignKey("ClienteId")]
        public virtual ClienteAD? Cliente { get; set; }
    }
}