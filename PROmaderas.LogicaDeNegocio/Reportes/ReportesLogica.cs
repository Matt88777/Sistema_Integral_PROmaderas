using ClosedXML.Excel;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Reportes
{
    // REP-HU-001: reporte de ventas por periodo (diario, semanal, mensual).
    public class ReportesLogica : IReportesLogica
    {
        private readonly IReportesRepositorio _reportesRepositorio;

        public ReportesLogica(IReportesRepositorio reportesRepositorio)
        {
            _reportesRepositorio = reportesRepositorio;
        }

        public async Task<VentasReporteResultadoDTO> GenerarReporteVentas(string? tipoPeriodo, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var periodo = PeriodosReporteVentas.EsValido(tipoPeriodo) ? tipoPeriodo! : PeriodosReporteVentas.Mensual;

            var hoy = DateTime.Today;

            // Escenario 2: si el usuario no indica fechas, se usa un rango razonable
            // según el tipo de periodo elegido (últimos 30 días, últimas 12 semanas
            // o últimos 12 meses).
            var inicioPorDefecto = periodo switch
            {
                PeriodosReporteVentas.Diario => hoy.AddDays(-29),
                PeriodosReporteVentas.Semanal => hoy.AddDays(-7 * 11),
                _ => new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-11)
            };

            var inicio = (fechaInicio ?? inicioPorDefecto).Date;
            var fin = (fechaFin ?? hoy).Date;

            if (inicio > fin)
                throw new ArgumentException("La fecha de inicio no puede ser mayor a la fecha final.");

            if ((fin - inicio).TotalDays > 366)
                throw new ArgumentException("El rango de fechas no puede ser mayor a un año.");

            var periodos = await _reportesRepositorio.ObtenerVentasPorPeriodo(periodo, inicio, fin);

            return new VentasReporteResultadoDTO
            {
                TipoPeriodo = periodo,
                FechaInicio = inicio,
                FechaFin = fin,
                Periodos = periodos
            };
        }
    }
}