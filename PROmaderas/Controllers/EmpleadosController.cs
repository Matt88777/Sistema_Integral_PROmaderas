using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class EmpleadosController : Controller
    {
        private readonly IEmpleadoLogica _empleadoLogica;
        private readonly IPuestoLogica _puestoLogica;

        public EmpleadosController(IEmpleadoLogica empleadoLogica, IPuestoLogica puestoLogica)
        {
            _empleadoLogica = empleadoLogica;
            _puestoLogica = puestoLogica;
        }

        public async Task<IActionResult> Index(string filtro, string departamento)
        {
            List<EmpleadoAD> empleados = await _empleadoLogica.ObtenerTodos();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                filtro = filtro.Trim().ToLower();
                empleados = empleados.Where(e =>
                    (!string.IsNullOrEmpty(e.Nombre) && e.Nombre.Trim().ToLower().Contains(filtro))
                    ||
                    (!string.IsNullOrEmpty(e.Cedula) && e.Cedula.Trim().ToLower().Contains(filtro))
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(departamento))
            {
                departamento = departamento.Trim().ToLower();
                empleados = empleados.Where(e =>
                    !string.IsNullOrEmpty(e.Departamento) &&
                    e.Departamento.Trim().ToLower().Contains(departamento)
                ).ToList();
            }

            return View(empleados);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmpleadoAD empleado)
        {
            if (!ModelState.IsValid)
                return View(empleado);

            await _empleadoLogica.Crear(empleado);
            return RedirectToAction(nameof(Index));
        }

        // GET: Empleados/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var empleado = (await _empleadoLogica.ObtenerTodos())
                .FirstOrDefault(e => e.IdEmpleado == id);

            if (empleado == null)
                return NotFound();

            await CargarPuestos(empleado.IdPuesto);
            return View(empleado);
        }

        // POST: Empleados/Edit/5
        // EMP-HU-003 - Actualizar informacion laboral del empleado (puesto, departamento) con auditoria.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmpleadoAD empleado)
        {
            // Ignorar campos que no se editan en este formulario
            ModelState.Remove(nameof(EmpleadoAD.FechaIngreso));
            ModelState.Remove(nameof(EmpleadoAD.FechaCreacion));
            ModelState.Remove(nameof(EmpleadoAD.Estado));
            ModelState.Remove(nameof(EmpleadoAD.PrimerApellido));
            ModelState.Remove(nameof(EmpleadoAD.SegundoApellido));

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor corrija los errores antes de guardar.";
                await CargarPuestos(empleado.IdPuesto);
                return View(empleado);
            }

            try
            {
                var contextoAuditoria = new ContextoAuditoria
                {
                    UsuarioIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Email = User.Identity?.Name,
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Accion = "Update"
                };

                await _empleadoLogica.Actualizar(empleado, contextoAuditoria);

                TempData["SuccessMessage"] = "La información del empleado fue actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al actualizar la información. Intente nuevamente.";
                await CargarPuestos(empleado.IdPuesto);
                return View(empleado);
            }
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CambiarEstado(int id)
		{
			try
			{
				var contextoAuditoria = new ContextoAuditoria
				{
					UsuarioIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier),
					Email = User.Identity?.Name,
					Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
					Accion = "Cambio de estado"
				};

				await _empleadoLogica.CambiarEstado(id, contextoAuditoria);

				TempData["SuccessMessage"] = "El estado del empleado fue actualizado correctamente.";
			}
			catch (Exception)
			{
				TempData["ErrorMessage"] = "Ocurrió un error al cambiar el estado del empleado.";
			}

			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var empleado = (await _empleadoLogica.ObtenerTodos())
                .FirstOrDefault(e => e.IdEmpleado == id);

            if (empleado == null)
                return NotFound();

            return View(empleado);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _empleadoLogica.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarPuestos(int seleccionado)
        {
            var puestos = await _puestoLogica.Listar();
            ViewBag.PuestoSelectList = new SelectList(puestos, "IdPuesto", "NombrePuesto", seleccionado);
        }
    }
}
