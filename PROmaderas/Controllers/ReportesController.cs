using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.UI.Models;
using PROmaderas.UI.Seguridad;
using PROmaderas.Abstracciones.Catalogos;


namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = Roles.Administrador + "," + Roles.Gerente + "," + Roles.Contador)]
    public class ReportesController : Controller
    {
        private readonly IReportesExportLogica _reportesExportLogica;
        private readonly IReportesLogica _reportesLogica;

        public ReportesController(IReportesExportLogica reportesExportLogica, IReportesLogica reportesLogica)
        {
            _reportesExportLogica = reportesExportLogica;
            _reportesLogica = reportesLogica;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ExportarFacturasExcel()
        {
            var bytes = await _reportesExportLogica.GenerarFacturasExcel();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Facturacion_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportarFacturasPdf()
        {
            var bytes = await _reportesExportLogica.GenerarFacturasPdf();
            return File(bytes, "application/pdf", $"Facturacion_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ExportarInventarioExcel()
        {
            var bytes = await _reportesExportLogica.GenerarInventarioExcel();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Inventario_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportarInventarioPdf()
        {
            var bytes = await _reportesExportLogica.GenerarInventarioPdf();
            return File(bytes, "application/pdf", $"Inventario_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ExportarPlanillaExcel()
        {
            var bytes = await _reportesExportLogica.GenerarPlanillaExcel();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Planilla_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportarPlanillaPdf()
        {
            var bytes = await _reportesExportLogica.GenerarPlanillaPdf();
            return File(bytes, "application/pdf", $"Planilla_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> Ventas(string? tipoPeriodo, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var modelo = new ReporteVentasViewModel
            {
                TipoPeriodo = PeriodosReporteVentas.EsValido(tipoPeriodo) ? tipoPeriodo! : PeriodosReporteVentas.Mensual,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            try
            {
                modelo.Resultado = await _reportesLogica.GenerarReporteVentas(tipoPeriodo, fechaInicio, fechaFin);
                modelo.TipoPeriodo = modelo.Resultado.TipoPeriodo;
                modelo.FechaInicio = modelo.Resultado.FechaInicio;
                modelo.FechaFin = modelo.Resultado.FechaFin;
                modelo.ConsultaRealizada = true;
            }
            catch (ArgumentException ex)
            {
                modelo.MensajeError = ex.Message;
            }

            return View(modelo);
        }

        public async Task<IActionResult> ExportarVentasExcel(string? tipoPeriodo, DateTime? fechaInicio, DateTime? fechaFin)
        {
            string periodo = PeriodosReporteVentas.EsValido(tipoPeriodo) ? tipoPeriodo! : PeriodosReporteVentas.Mensual;
            DateTime inicio = fechaInicio ?? DateTime.Today.AddMonths(-12);
            DateTime fin = fechaFin ?? DateTime.Today;
            var bytes = await _reportesExportLogica.GenerarVentasExcel(periodo, inicio, fin);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Ventas_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportarVentasPdf(string? tipoPeriodo, DateTime? fechaInicio, DateTime? fechaFin)
        {
            string periodo = PeriodosReporteVentas.EsValido(tipoPeriodo) ? tipoPeriodo! : PeriodosReporteVentas.Mensual;
            DateTime inicio = fechaInicio ?? DateTime.Today.AddMonths(-12);
            DateTime fin = fechaFin ?? DateTime.Today;
            var bytes = await _reportesExportLogica.GenerarVentasPdf(periodo, inicio, fin);
            return File(bytes, "application/pdf", $"Ventas_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}