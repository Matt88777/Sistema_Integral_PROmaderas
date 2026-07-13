using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    // PLA-HU-012: acumuladas / disfrutadas / saldo.
    public interface IVacacionLogica
    {
        // ── Pantallas ─────────────────────────────────────────────────────────

        // Listado: todos los empleados activos con su saldo. Resuelve el parámetro
        // 'DiasVacacionesPorMes' UNA sola vez para todo el listado.
        // Lanza InvalidOperationException si el parámetro no tiene versión vigente hoy.
        Task<List<SaldoVacacionesAD>> ObtenerListado();

        // Saldo a HOY: lo que consume la pantalla de Vacaciones.
        Task<SaldoVacacionesAD> ObtenerSaldo(int idEmpleado);

        // PLA-HU-017: saldo a una FECHA DE CORTE (la fecha de salida de una liquidación).
        // Los MESES TRABAJADOS se cuentan contra esa fecha, no contra hoy: liquidar a alguien
        // que salió en marzo no puede acreditarle los meses de abril en adelante.
        // El parámetro 'DiasVacacionesPorMes' se sigue resolviendo a hoy (ver VacacionLogica).
        Task<SaldoVacacionesAD> ObtenerSaldo(int idEmpleado, DateTime fechaCorte);

        // Todas las vacaciones del empleado, incluidas las anuladas.
        Task<List<VacacionAD>> ObtenerHistorial(int idEmpleado);

        // ── Escrituras (las 2 se auditan) ─────────────────────────────────────

        // 'dias' se DIGITA: no se deriva del rango de fechas. Se bloquea si dejaría el
        // saldo en negativo, o si el empleado no tiene fecha de ingreso registrada.
        Task Registrar(int idEmpleado, DateTime inicio, DateTime fin, decimal dias,
                       string observacion, ContextoAuditoria auditoria);

        // Motivo obligatorio: queda en la bitácora.
        Task Anular(int idVacacion, string motivo, ContextoAuditoria auditoria);
    }
}
