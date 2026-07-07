using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.LogicaDeNegocio.Deducciones;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
    public class DeduccionesController : Controller
    {
        private readonly IDeduccionInternaLogica _logica;
        private readonly IEmpleadoLogica _empleadoLogica;

        public DeduccionesController(IDeduccionInternaLogica logica, IEmpleadoLogica empleadoLogica)
        {
            _logica = logica;
            _empleadoLogica = empleadoLogica;
        }

        public async Task<IActionResult> Index()
        {
            var lista = await _logica.ObtenerTodas();
            return View(lista);
        }

        public IActionResult Create() => View(new DeduccionInternaAD());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeduccionInternaAD model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                await _logica.Crear(model);
                TempData["SuccessMessage"] = "Deducción creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var d = await _logica.ObtenerPorId(id);
            if (d == null) return NotFound();
            return View(d);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DeduccionInternaAD model)
        {
            if (!ModelState.IsValid) return View(model);
            await _logica.Actualizar(model);
            TempData["SuccessMessage"] = "Deducción actualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _logica.Eliminar(id);
            TempData["SuccessMessage"] = "Deducción eliminada.";
            return RedirectToAction(nameof(Index));
        }

        // ── Asignación por empleado ─────────────────────────────────────────

        public async Task<IActionResult> Asignar(int idEmpleado)
        {
            var empleado = (await _empleadoLogica.ObtenerTodos())
                .FirstOrDefault(e => e.IdEmpleado == idEmpleado);
            if (empleado == null) return NotFound();

            var asignadas = await _logica.ObtenerAsignacionesPorEmpleado(idEmpleado);
            var todasDed = await _logica.ObtenerTodas();
            var idsAsig = asignadas.Select(a => a.IdDeduccion).ToHashSet();
            var disponibles = todasDed.Where(d => d.Activa && !idsAsig.Contains(d.IdDeduccion)).ToList();

            ViewBag.Empleado = empleado;
            ViewBag.Disponibles = disponibles;
            return View(asignadas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarDeduccion(int idEmpleado, int idDeduccion)
        {
            await _logica.AsignarAEmpleado(idEmpleado, idDeduccion);
            return RedirectToAction(nameof(Asignar), new { idEmpleado });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desasignar(int idEmpleadoDeduccion, int idEmpleado)
        {
            await _logica.DesasignarDeEmpleado(idEmpleadoDeduccion);
            return RedirectToAction(nameof(Asignar), new { idEmpleado });
        }
    }
}