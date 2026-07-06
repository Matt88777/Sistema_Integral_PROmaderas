using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
    public class HistorialPagosController : Controller
    {
        private readonly IHistorialPagosLogica _historialPagosLogica;

        public HistorialPagosController(IHistorialPagosLogica historialPagosLogica)
        {
            _historialPagosLogica = historialPagosLogica;
        }

        public async Task<IActionResult> Index(int? idEmpleado)
        {
            var empleados = await _historialPagosLogica.ObtenerEmpleados();
            ViewBag.EmpleadoSelectList = new SelectList(empleados, "IdEmpleado", "Nombre", idEmpleado);

            if (idEmpleado.HasValue)
            {
                var historial = await _historialPagosLogica.ObtenerHistorialPorEmpleado(idEmpleado.Value);
                ViewBag.IdEmpleadoSeleccionado = idEmpleado.Value;
                return View(historial);
            }

            return View(new List<PROmaderas.Abstracciones.Models.PlanillaDetalleFinancieroAD>());
        }
    }
}