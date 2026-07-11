using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.UI.Models;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    // PLA-HU-019: configurar parámetros de planilla. La HU dice "Como Administrador":
    // el Contador NO entra acá, aunque sí entre a Planilla y Deducciones.
    [Authorize(Roles = Roles.Administrador)]
    public class ParametrosController : Controller
    {
        private readonly IParametroPlanillaLogica _logica;

        public ParametrosController(IParametroPlanillaLogica logica)
        {
            _logica = logica;
        }

        public async Task<IActionResult> Index()
        {
            // DateTime.Today lo pone la UI: ni la Lógica ni el Acceso a Datos deciden "hoy".
            var vm = await _logica.ObtenerListado(DateTime.Today);
            return View(vm);
        }

        public async Task<IActionResult> Historial(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return NotFound();

            var versiones = await _logica.ObtenerHistorial(nombre);
            if (!versiones.Any()) return NotFound();

            var vigente = await _logica.ObtenerVersionVigente(nombre, DateTime.Today);

            ViewBag.Nombre = nombre;
            // Id de la versión que rige hoy: la vista la marca distinto (no cambia el @model).
            ViewBag.IdVigente = vigente?.IdParametroPlanilla;
            return View(versiones);
        }

        public IActionResult Create() => View(new CrearParametroViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearParametroViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                await _logica.Crear(vm.NombreParametro, vm.Valor, vm.FechaInicio, ArmarContexto(
                    AccionesAuditoria.CrearParametro));

                TempData["SuccessMessage"] = $"Parámetro {vm.NombreParametro} creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        public async Task<IActionResult> NuevaVersion(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return NotFound();

            var historial = await _logica.ObtenerHistorial(nombre);
            if (!historial.Any()) return NotFound();

            return View(await ArmarNuevaVersionVm(nombre, new NuevaVersionParametroViewModel()));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevaVersion(NuevaVersionParametroViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(await ArmarNuevaVersionVm(vm.NombreParametro, vm));

            try
            {
                await _logica.CrearNuevaVersion(vm.NombreParametro, vm.Valor, vm.FechaInicio,
                    vm.Motivo, ArmarContexto(AccionesAuditoria.NuevaVersionParametro));

                TempData["SuccessMessage"] =
                    $"Nueva versión de {vm.NombreParametro} vigente desde el {vm.FechaInicio:dd/MM/yyyy}. " +
                    "La versión anterior se cerró y sigue válida para su período.";
                return RedirectToAction(nameof(Historial), new { nombre = vm.NombreParametro });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(await ArmarNuevaVersionVm(vm.NombreParametro, vm));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AnularVersion(int idVersion, string nombre, string motivo)
        {
            try
            {
                await _logica.AnularVersion(idVersion, motivo, ArmarContexto(
                    AccionesAuditoria.AnularVersionParametro));

                TempData["SuccessMessage"] = "La versión fue anulada y ya no se resuelve como vigente.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Historial), new { nombre });
        }

        // La identidad viaja en el DTO: la Lógica y el Acceso a Datos no conocen Identity
        // ni HttpContext. IdUsuario queda NULL; el actor lo mete ConstructorBitacora bajo
        // "modificadoPor" dentro de ValorNuevo.
        private ContextoAuditoria ArmarContexto(string accion) => new()
        {
            UsuarioIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.Identity?.Name,
            Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Accion = accion
        };

        // Rellena el contexto de la pantalla desde la BD (qué rige hoy), conservando lo que
        // el usuario escribió.
        private async Task<NuevaVersionParametroViewModel> ArmarNuevaVersionVm(
            string nombre, NuevaVersionParametroViewModel vm)
        {
            var vigente = await _logica.ObtenerVersionVigente(nombre, DateTime.Today);

            vm.NombreParametro = nombre;
            vm.ValorActual = vigente?.Valor;
            vm.VigenteDesde = vigente?.FechaInicio;
            return vm;
        }
    }
}
