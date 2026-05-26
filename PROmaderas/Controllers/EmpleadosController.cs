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
        public async Task<IActionResult> Edit(int id)
        {
            var empleado = (await _repositorio.ObtenerTodos())
                .FirstOrDefault(e => e.IdEmpleado == id);
            if (empleado == null)
                return NotFound();
            return View(empleado);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmpleadoAD empleado)
        {
            ModelState.Remove(nameof(EmpleadoAD.FechaIngreso));
            ModelState.Remove(nameof(EmpleadoAD.FechaCreacion));
            ModelState.Remove(nameof(EmpleadoAD.IdPuesto));
            ModelState.Remove(nameof(EmpleadoAD.Estado));
            ModelState.Remove(nameof(EmpleadoAD.PrimerApellido));
            ModelState.Remove(nameof(EmpleadoAD.SegundoApellido));
            ModelState.Remove(nameof(EmpleadoAD.Puesto));

            if (!ModelState.IsValid)
                return View(empleado);

            await _repositorio.Actualizar(empleado);
            return RedirectToAction(nameof(Index));
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