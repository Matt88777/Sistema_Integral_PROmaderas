using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos.Empleados;

namespace PROmaderas.UI.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly EmpleadoRepositorio _repositorio;

        public EmpleadosController(EmpleadoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<IActionResult> Index(string filtro, string departamento)
        {
            List<EmpleadoAD> empleados = await _repositorio.ObtenerTodos();

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

            await _repositorio.Crear(empleado);
            return RedirectToAction(nameof(Index));
        }

        // GET: Empleados/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var empleado = (await _repositorio.ObtenerTodos())
                .FirstOrDefault(e => e.IdEmpleado == id);

            if (empleado == null)
                return NotFound();

            return View(empleado);
        }

        // POST: Empleados/Edit/5
        // EMP-HU-007 – Actualizar información personal del empleado
        // Escenario 1: Actualización exitosa  → datos válidos → guarda y redirige con mensaje de confirmación
        // Escenario 2: Datos inválidos         → ModelState inválido → regresa a la vista con errores
        // Escenario 3: Confirmación de cambios → actualización exitosa → muestra TempData de confirmación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmpleadoAD empleado)
        {
            // Ignorar campos que no se editan en este formulario
            ModelState.Remove(nameof(EmpleadoAD.FechaIngreso));
            ModelState.Remove(nameof(EmpleadoAD.FechaCreacion));
            ModelState.Remove(nameof(EmpleadoAD.IdPuesto));
            ModelState.Remove(nameof(EmpleadoAD.Estado));
            ModelState.Remove(nameof(EmpleadoAD.PrimerApellido));
            ModelState.Remove(nameof(EmpleadoAD.SegundoApellido));
            ModelState.Remove(nameof(EmpleadoAD.Puesto));

            // Escenario 2: Datos inválidos – se muestran mensajes de error en la vista
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor corrija los errores antes de guardar.";
                return View(empleado);
            }

            try
            {
                // Escenario 1: Actualización exitosa – se persisten los cambios
                await _repositorio.Actualizar(empleado);

                // Escenario 3: Confirmación de cambios – mensaje de éxito para mostrar en Index
                TempData["SuccessMessage"] = "La información del empleado fue actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al actualizar la información. Intente nuevamente.";
                return View(empleado);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var empleado = (await _repositorio.ObtenerTodos())
                .FirstOrDefault(e => e.IdEmpleado == id);

            if (empleado == null)
                return NotFound();

            return View(empleado);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repositorio.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }
    }
}