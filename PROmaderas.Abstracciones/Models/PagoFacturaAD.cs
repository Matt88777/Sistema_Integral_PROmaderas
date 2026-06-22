using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    [Table("PagoFactura")]
    public class PagoFacturaAD
    {
        [Key]
        public int IdPagoFactura { get; set; }

        [Required]
        public int IdFactura { get; set; }

        [Required]
        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        [StringLength(50)]
        public string FormaPago { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Referencia { get; set; }

        [Required]
        public int IdUsuarioRegistro { get; set; }

        // Navegación opcional a la factura (FK IdFactura -> Factura.IdFactura).
        [ForeignKey("IdFactura")]
        public virtual FacturacionAD? Factura { get; set; }
    }
}
