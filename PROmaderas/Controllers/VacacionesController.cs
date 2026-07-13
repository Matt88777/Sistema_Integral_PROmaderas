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
    // PLA-HU-012: vacaciones acumuladas / disfrutadas / saldo. La HU dice "Como Administrador".
    [Authorize(Roles = Roles.Administrador)]
    public class VacacionesController : Controller
    {
        private readonly IVacacionLogica _logica;

        public VacacionesController(IVacacionLogica logica)
        {
            _logica = logica;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _logica.ObtenerListado());
            }
            catch (InvalidOperationException ex)
            {
                // Falta la versión vigente de 'DiasVacacionesPorMes': sin ese parámetro no hay
                // acumulado que calcular. Se muestra el mensaje en un alert rojo, con la lista
                // vacía, en vez de tirarle al usuario la pantalla amarilla de error.
                ViewBag.ErrorParametro = ex.Message;
                return View(new List<SaldoVacacionesAD>());
            }
        }

        public async Task<IActionResult> Detalle(int idEmpleado)
        {
            try
            {
                ViewBag.Saldo = await _logica.ObtenerSaldo(idEmpleado);
                return View(await _logica.ObtenerHistorial(idEmpleado));
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create(int idEmpleado)
        {
            try
            {
                var saldo = await _logica.ObtenerSaldo(idEmpleado);

                return View(new RegistrarVacacionViewModel
                {
                    IdEmpleado = saldo.IdEmpleado,
                    NombreEmpleado = saldo.NombreCompleto,
                    SaldoDisponible = saldo.Saldo
                });
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegistrarVacacionViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(await RellenarContexto(vm));

            try
            {
                await _logica.Registrar(vm.IdEmpleado, vm.FechaInicio, vm.FechaFin, vm.Dias,
                    vm.Observacion ?? string.Empty, ArmarContexto(AccionesAuditoria.RegistrarVacacion));

                TempData["SuccessMessage"] =
                    $"Se registraron {vm.Dias:N2} día(s) de vacaciones del " +
                    $"{vm.FechaInicio:dd/MM/yyyy} al {vm.FechaFin:dd/MM/yyyy}.";

                return RedirectToAction(nameof(Detalle), new { idEmpleado = vm.IdEmpleado });
            }
            catch (ArgumentException ex)
            {
                // Incluye el bloqueo por saldo negativo y el de empleado sin fecha de ingreso.
                ModelState.AddModelError("", ex.Message);
                return View(await RellenarContexto(vm));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(await RellenarContexto(vm));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int idVacacion, int idEmpleado, string motivo)
        {
            try
            {
                await _logica.Anular(idVacacion, motivo, ArmarContexto(AccionesAuditoria.AnularVacacion));

                TempData["SuccessMessage"] =
                    "La vacación fue anulada y sus días volvieron al saldo del empleado.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { idEmpleado });
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

        // El nombre y el saldo que muestra el formulario se releen de la BD: lo que venga en el
        // POST es solo lo que el usuario digitó, no un dato en el que se pueda confiar.
        private async Task<RegistrarVacacionViewModel> RellenarContexto(RegistrarVacacionViewModel vm)
        {
            try
            {
                var saldo = await _logica.ObtenerSaldo(vm.IdEmpleado);
                vm.NombreEmpleado = saldo.NombreCompleto;
                vm.SaldoDisponible = saldo.Saldo;
            }
            catch (ArgumentException)
            {
                // El empleado no existe: el mensaje ya está en el ModelState, la vista se pinta
                // sin el encabezado de contexto en vez de reventar acá.
            }

            return vm;
        }
    }
}
