namespace PROmaderas.Abstracciones.Models
{
    // PLA-HU-017: read-model del DESGLOSE (Escenarios 2 y 3). No mapea a ninguna tabla.
    //
    // Lo usa la preview (calcular sin guardar) y también el Detalle de una liquidación ya
    // guardada, para que el contador vea siempre el mismo desglose rubro por rubro, con las
    // cifras intermedias que explican cada monto (salario/día, años topados, rango del
    // aguinaldo). Sin esas cifras el desglose es una lista de totales que nadie puede auditar.
    public class LiquidacionCalculoAD
    {
        // Solo lo trae el Detalle de una liquidación guardada; en la preview es null.
        public int? IdLiquidacion { get; set; }
        public string? EstadoLiquidacion { get; set; }

        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public string? Cedula { get; set; }

        public DateTime FechaIngreso { get; set; }
        public DateTime FechaSalida { get; set; }
        public string MotivoSalida { get; set; } = string.Empty;

        public decimal AniosLaborados { get; set; }

        // El salario mensual congelado y su derivado diario (SalarioPromedio / DiasMes).
        public decimal SalarioPromedio { get; set; }
        public decimal SalarioDia { get; set; }

        // ── Rubro 1: vacaciones (SIEMPRE se pagan) ────────────────────────────
        public decimal DiasVacacionesPendientes { get; set; }
        public decimal MontoVacaciones { get; set; }

        // ── Rubro 2: aguinaldo proporcional (SIEMPRE se paga) ─────────────────
        // El rango y el acumulado bruto quedan expuestos para que el desglose muestre la
        // fórmula: no es SalarioBase, son los salarios realmente devengados en planilla.
        public DateTime AguinaldoDesde { get; set; }
        public DateTime AguinaldoHasta { get; set; }
        public decimal AguinaldoBrutosDevengados { get; set; }
        public decimal MontoAguinaldoProporcional { get; set; }

        // ── Rubro 3: preaviso (solo si el motivo lo genera) ───────────────────
        // GeneraPreaviso == false -> el monto es 0 y la vista dice POR QUÉ (Escenario 3).
        public bool GeneraPreaviso { get; set; }
        public decimal PreavisoDias { get; set; }
        public decimal MontoPreaviso { get; set; }

        // ── Rubro 4: cesantía (solo si el motivo la genera) ───────────────────
        public bool GeneraCesantia { get; set; }
        public decimal CesantiaDiasPorAnio { get; set; }
        public decimal CesantiaTopeAnios { get; set; }
        public decimal CesantiaAniosReconocidos { get; set; }   // MIN(años, tope)
        public decimal MontoCesantia { get; set; }

        // ── Otros y total ─────────────────────────────────────────────────────
        public decimal OtrosMontos { get; set; }

        // Calculado, no almacenado: así la preview puede recalcular el total en cuanto el
        // controller le asigna OtrosMontos, sin duplicar la suma en dos lugares.
        public decimal Total =>
            MontoVacaciones
            + MontoAguinaldoProporcional
            + MontoPreaviso
            + MontoCesantia
            + OtrosMontos;
    }
}
