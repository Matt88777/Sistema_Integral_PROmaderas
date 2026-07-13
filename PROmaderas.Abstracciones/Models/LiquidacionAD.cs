using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
    // PLA-HU-017: la liquidación es un DOCUMENTO que se emite y se conserva, no un cálculo al
    // vuelo (lo dice el script que creó la tabla).
    //
    // Por eso FechaIngreso, FechaSalida, MotivoSalida y SalarioPromedio se CONGELAN acá, aunque
    // FechaIngreso y el salario ya vivan en Empleado: si mañana alguien corrige la fecha de
    // ingreso del empleado, o el admin cambia los parámetros de planilla, la liquidación ya
    // emitida tiene que seguir diciendo exactamente lo que se le pagó a esa persona. Es
    // evidencia, no una vista.
    //
    // Una liquidación no se edita: se ANULA (Estado = 'Anulada', Activa = 0) y se vuelve a
    // calcular, igual que las facturas y las versiones de parámetro.
    [Table("Liquidacion")]
    public class LiquidacionAD
    {
        [Key]
        public int IdLiquidacion { get; set; }

        public int IdEmpleado { get; set; }

        public DateTime FechaCalculo { get; set; }

        // Congelados al calcular (ver comentario de arriba).
        [Column(TypeName = "date")]
        public DateTime FechaIngreso { get; set; }

        [Column(TypeName = "date")]
        public DateTime FechaSalida { get; set; }

        [Required]
        [StringLength(50)]
        public string MotivoSalida { get; set; } = string.Empty;   // siempre de MotivosSalida

        [Column(TypeName = "decimal(10,2)")]
        public decimal AniosLaborados { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalarioPromedio { get; set; }

        // ── Los 4 rubros de la HU ─────────────────────────────────────────────

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiasVacacionesPendientes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoVacaciones { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoAguinaldoProporcional { get; set; }

        // Quedan en 0 cuando el motivo no los genera (Escenario 3). No se ocultan: un cero
        // explícito es evidencia de que se evaluó el rubro y no correspondía.
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoPreaviso { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoCesantia { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtrosMontos { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalLiquidacion { get; set; }

        // ── Estado y trazabilidad ─────────────────────────────────────────────

        // Siempre uno de EstadosLiquidacion. Esta columna SÍ tiene CHECK en la BD.
        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Observacion { get; set; }

        // NOT NULL con FK a dbo.Usuario (int), no a Identity (GUID). Se resuelve por correo,
        // con fallback a admin, igual que IdUsuarioEmisor en Factura.
        public int IdUsuarioRegistro { get; set; }

        // Soft-delete: la fila anulada queda como evidencia, pero deja de contar.
        public bool Activa { get; set; } = true;
    }
}
