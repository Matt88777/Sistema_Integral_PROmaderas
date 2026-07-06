using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class LicenciasController : Controller
    {
        private readonly ILicenciaLogica _licenciaLogica;
        private readonly IEmpleadoLogica _empleadoLogica;

        public LicenciasController(ILicenciaLogica licenciaLogica, IEmpleadoLogica empleadoLogica)
        {
            _licenciaLogica = licenciaLogica;
            _empleadoLogica = empleadoLogica;
        }

        public async Task<IActionResult> Index()
        {
            var licencias = await _licenciaLogica.ObtenerTodas();
            return View(licencias);
        }

        public async Task<IActionResult> Create()
        {
            await CargarEmpleados(null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LicenciaAD licencia)
        {
            if (!ModelState.IsValid)
            {
                await CargarEmpleados(licencia.IdEmpleado);
                return View(licencia);
            }

            try
            {
                await _licenciaLogica.Registrar(licencia);
                TempData["SuccessMessage"] = "La licencia fue registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                await CargarEmpleados(licencia.IdEmpleado);
                return View(licencia);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var licencia = await _licenciaLogica.ObtenerPorId(id);
            if (licencia == null)
                return NotFound();

            await CargarEmpleados(licencia.IdEmpleado);
            return View(licencia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LicenciaAD licencia)
        {
            if (!ModelState.IsValid)
            {
                await CargarEmpleados(licencia.IdEmpleado);
                return View(licencia);
            }

            try
            {
                await _licenciaLogica.Actualizar(licencia);
                TempData["SuccessMessage"] = "La licencia fue actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                await CargarEmpleados(licencia.IdEmpleado);
                return View(licencia);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var licencia = await _licenciaLogica.ObtenerPorId(id);
            if (licencia == null)
                return NotFound();

            return View(licencia);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _licenciaLogica.Eliminar(id);
            TempData["SuccessMessage"] = "La licencia fue eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarEmpleados(int? seleccionado)
        {
            var empleados = await _empleadoLogica.ObtenerTodos();
            ViewBag.EmpleadoSelectList = new SelectList(empleados, "IdEmpleado", "Nombre", seleccionado);
        }
    }
}