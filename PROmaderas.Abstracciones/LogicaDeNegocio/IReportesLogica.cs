using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    // REP-HU-001: reporte de ventas por periodo (diario, semanal, mensual).
    public interface IReportesLogica
    {
        /// <summary>
        /// Genera el reporte de ventas agrupado por periodo (Escenario 2).
        /// Si <paramref name="tipoPeriodo"/> es nulo/ inválido, se usa "Mensual".
        /// Si <paramref name="fechaInicio"/>/<paramref name="fechaFin"/> son nulos,
        /// se calcula un rango por defecto según el periodo elegido.
        /// El resultado trae <c>HayDatos = false</c> cuando no existen ventas (Escenario 3).
        /// </summary>
        Task<VentasReporteResultadoDTO> GenerarReporteVentas(string? tipoPeriodo, DateTime? fechaInicio, DateTime? fechaFin);

        /// <summary>
        /// Genera el reporte de inventario con existencias actuales (Escenario 1),
        /// movimientos de entradas/salidas (Escenario 2) y el resumen de productos
        /// con bajo stock (Escenario 3).
        /// </summary>
        Task<ReporteInventarioResultadoDTO> GenerarReporteInventario();
    }
}
