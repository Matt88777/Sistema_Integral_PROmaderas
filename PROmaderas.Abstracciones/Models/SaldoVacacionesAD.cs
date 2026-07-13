namespace PROmaderas.Abstracciones.Models
{
    // PLA-HU-012: read-model de la pantalla de vacaciones. No mapea a ninguna tabla:
    // el saldo es un CÁLCULO, no una columna.
    //
    //   Acumuladas  = SaldoInicial + (MesesTrabajados * parámetro 'DiasVacacionesPorMes')
    //   Disfrutadas = SUM(Vacacion.Dias) de las que están en estado 'Disfrutada'
    //   Saldo       = Acumuladas - Disfrutadas
    public class SaldoVacacionesAD
    {
        public int IdEmpleado { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Cedula { get; set; }

        // Nullable a propósito: Empleado.FechaIngreso lo es. Ver TieneFechaIngreso.
        public DateTime? FechaIngreso { get; set; }

        public decimal SaldoInicial { get; set; }
        public int MesesTrabajados { get; set; }
        public decimal Acumuladas { get; set; }
        public decimal Disfrutadas { get; set; }
        public decimal Saldo { get; set; }

        // Un empleado SIN fecha de ingreso no tiene con qué calcular meses trabajados.
        // No se revienta el listado por eso: la fila se pinta con "—" y una advertencia,
        // y Acumuladas/Saldo quedan en 0 (no son un número real, la vista no los muestra).
        // Registrarle una vacación sí se bloquea: ahí el dato faltante sí importa.
        public bool TieneFechaIngreso { get; set; }
    }
}
