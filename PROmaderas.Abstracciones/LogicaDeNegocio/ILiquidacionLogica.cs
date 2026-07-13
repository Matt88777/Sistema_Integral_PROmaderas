using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    // PLA-HU-017: liquidación de empleados.
    public interface ILiquidacionLogica
    {
        // ── Pantallas ─────────────────────────────────────────────────────────

        Task<List<LiquidacionAD>> ObtenerTodas();

        Task<LiquidacionAD?> ObtenerPorId(int id);

        // Solo los activos: el combo de "nueva liquidación".
        Task<List<EmpleadoAD>> ObtenerEmpleadosLiquidables();

        // Id -> nombre de TODOS los empleados (el liquidado queda inactivo), para el listado.
        Task<Dictionary<int, string>> ObtenerNombresEmpleados();

        // El desglose de una liquidación YA GUARDADA, reconstruido desde los montos congelados
        // en la fila. NO recalcula nada: si los parámetros cambiaron, la liquidación emitida
        // sigue diciendo lo mismo (Escenario 2).
        Task<LiquidacionCalculoAD?> ObtenerDesglose(int id);

        // ── Cálculo ───────────────────────────────────────────────────────────

        // PREVIEW: calcula y devuelve el desglose, NO guarda nada.
        // Lanza ArgumentException si el empleado/motivo/fecha no son válidos, e
        // InvalidOperationException si falta un parámetro vigente o el empleado no tiene
        // planillas en el rango del aguinaldo.
        Task<LiquidacionCalculoAD> Calcular(int idEmpleado, DateTime fechaSalida, string motivo);

        // ── Escrituras (las 2 se auditan) ─────────────────────────────────────

        // RECALCULA todo en el servidor desde (idEmpleado, fechaSalida, motivo): los montos que
        // venga a mandar el formulario se ignoran. Del form solo se aceptan otrosMontos y la
        // observación.
        Task Guardar(int idEmpleado, DateTime fechaSalida, string motivo, decimal otrosMontos,
                     string? observacion, ContextoAuditoria auditoria);

        // Motivo obligatorio: queda en la bitácora. Reabre al empleado.
        Task Anular(int idLiquidacion, string motivo, ContextoAuditoria auditoria);
    }
}
