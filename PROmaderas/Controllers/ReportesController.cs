using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = Roles.Administrador + "," + Roles.Gerente + "," + Roles.Contador)]
    public class ReportesController : Controller
    {
        private readonly IReportesExportLogica _reportesExportLogica;

        public ReportesController(IReportesExportLogica reportesExportLogica)
        {
            _reportesExportLogica = reportesExportLogica;
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
    }
}