using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
    public class PlanillaController : Controller
    {
        private readonly IPlanillaLogica _planillaLogica;
        private readonly IParametroPlanillaLogica _parametroLogica;
        private const int TamanioPagina = 8;

        public PlanillaController(IPlanillaLogica planillaLogica,
                                  IParametroPlanillaLogica parametroLogica)
        {
            _planillaLogica = planillaLogica;
            _parametroLogica = parametroLogica;
        }

        public async Task<IActionResult> Index(int pagina = 1)
        {
            var periodos = await _planillaLogica.ObtenerPeriodos();

            int totalPaginas = (int)Math.Ceiling(periodos.Count / (double)TamanioPagina);
            if (totalPaginas < 1) totalPaginas = 1;
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas) pagina = totalPaginas;

            var periodosPagina = periodos
                .Skip((pagina - 1) * TamanioPagina)
                .Take(TamanioPagina)
                .ToList();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(periodosPagina);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var periodo = await _planillaLogica.ObtenerPeriodoPorId(id.Value);
            if (periodo == null) return NotFound();

            ViewBag.EmpleadosDisponibles = await _planillaLogica.ObtenerEmpleadosActivos();

            // PLA-HU-019: el % de CCSS que la cabecera muestra es el que RIGE PARA ESTE PERÍODO
            // (misma fecha de resolución que usa el cálculo), no un literal en la vista.
            ViewBag.PorcentajeCCSS = await _parametroLogica.ObtenerValorVigente(
                ParametrosPlanilla.PorcentajeCCSS, periodo.FechaInicio);

            return View(periodo);
        }

        public IActionResult Create()
        {
            return View(new PlanillaPeriodoAD
            {
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddDays(14)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlanillaPeriodoAD periodo)
        {
            if (periodo.FechaFin < periodo.FechaInicio)
                ModelState.AddModelError(nameof(periodo.FechaFin), "La fecha de fin no puede ser anterior a la fecha de inicio.");

            if (!ModelState.IsValid)
                return View(periodo);

            try
            {
                periodo.IdUsuarioCreacion = ObtenerIdUsuarioActual();
                await _planillaLogica.CrearPeriodo(periodo);
                TempData["SuccessMessage"] = "Período de planilla creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al crear el período: {ex.Message}");
                return View(periodo);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarHoras(PlanillaDetalleFormVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Revisa los datos ingresados: " +
                    string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id = vm.IdPlanillaPeriodo });
            }

            try
            {
                await _planillaLogica.RegistrarHoras(vm, ObtenerContextoAuditoria("Create"));
                TempData["SuccessMessage"] = "Horas registradas y salario bruto calculado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = vm.IdPlanillaPeriodo });
        }
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditarHoras(int idPlanillaDetalle, int idPeriodo, decimal salarioMensual, decimal horasOrdinarias, decimal horasExtra)
{
    try
    {
        await _planillaLogica.ActualizarHoras(idPlanillaDetalle, salarioMensual, horasOrdinarias, horasExtra, ObtenerContextoAuditoria("Update"));
        TempData["SuccessMessage"] = "Horas actualizadas y salario recalculado correctamente.";
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = ex.Message;
    }

    return RedirectToAction(nameof(Details), new { id = idPeriodo });
}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarDetalle(int idDetalle, int idPeriodo)
        {
            await _planillaLogica.EliminarDetalle(idDetalle);
            TempData["SuccessMessage"] = "Registro de horas eliminado.";
            return RedirectToAction(nameof(Details), new { id = idPeriodo });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            try
            {
                await _planillaLogica.CambiarEstadoPeriodo(id, nuevoEstado, ObtenerContextoAuditoria("Update"));
                TempData["SuccessMessage"] = $"El período ahora está en estado '{nuevoEstado}'.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var periodo = await _planillaLogica.ObtenerPeriodoPorId(id.Value);
            if (periodo == null) return NotFound();

            return View(periodo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _planillaLogica.EliminarPeriodo(id);
            TempData["SuccessMessage"] = "Período de planilla eliminado.";
            return RedirectToAction(nameof(Index));
        }

        private int ObtenerIdUsuarioActual()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : 1;
        }

        private ContextoAuditoria ObtenerContextoAuditoria(string accion)
        {
            return new ContextoAuditoria
            {
                UsuarioIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Email = User.Identity?.Name,
                Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Accion = accion
            };
        }
    }
}