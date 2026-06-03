using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    [Table("BitacoraAuditoria")]
    public class BitacoraAuditoriaAD
    {
        [Key]
        public int IdBitacora { get; set; }

        public int? IdUsuario { get; set; }

        [StringLength(100)]
        public string TablaAfectada { get; set; } = string.Empty;

        public int? IdRegistroAfectado { get; set; }

        [StringLength(50)]
        public string Accion { get; set; } = string.Empty;

        public string? ValorAnterior { get; set; }

        public string? ValorNuevo { get; set; }

        public DateTime FechaAccion { get; set; }

        [StringLength(50)]
        public string? DireccionIP { get; set; }
    }
}
