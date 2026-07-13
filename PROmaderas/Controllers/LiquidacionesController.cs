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
    // PLA-HU-017: la HU dice "Como Contador". Mismo patrón que AguinaldoController.
    [Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
    public class LiquidacionesController : Controller
    {
        private readonly ILiquidacionLogica _logica;

        public LiquidacionesController(ILiquidacionLogica logica)
        {
            _logica = logica;
        }

        public async Task<IActionResult> Index()
        {
            var liquidaciones = await _logica.ObtenerTodas();

            // Los nombres no viven en Liquidacion (solo el IdEmpleado). Se resuelven con UN query
            // que trae a TODOS los empleados: el liquidado queda inactivo, así que buscarlos entre
            // los activos dejaría sin nombre justo a las filas de esta pantalla.
            ViewBag.Nombres = await _logica.ObtenerNombresEmpleados();

            return View(liquidaciones);
        }

        // [HttpGet] EXPLÍCITO: sin él, una acción sin atributo de verbo responde también a POST.
        // Un POST del formulario que no llegara a Calcular caía acá, y esta acción descarta el
        // modelo posteado y devuelve uno vacío: la pantalla se re-renderizaba sin calcular y sin
        // ningún mensaje. Un error silencioso es peor que una pantalla de error.
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarCombos();
            return View(new CalcularLiquidacionViewModel());
        }

        // PREVIEW: calcula y muestra el desglose. NO guarda nada.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Calcular(CalcularLiquidacionViewModel vm)
        {
            await CargarCombos();

            if (!ModelState.IsValid)
                return View(nameof(Create), vm);

            try
            {
                var calculo = await _logica.Calcular(vm.IdEmpleado, vm.FechaSalida, vm.MotivoSalida);

                // OtrosMontos lo aporta el contador; el Total del read-model lo suma solo.
                calculo.OtrosMontos = vm.OtrosMontos;
                vm.Calculo = calculo;

                return View(nameof(Create), vm);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(nameof(Create), vm);
            }
            catch (InvalidOperationException ex)
            {
                // Falta un parámetro vigente, o el empleado no tiene planillas en el rango del
                // aguinaldo. Se muestra el mensaje, no una pantalla amarilla.
                ModelState.AddModelError("", ex.Message);
                return View(nameof(Create), vm);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(CalcularLiquidacionViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombos();
                return View(nameof(Create), vm);
            }

            try
            {
                // La Lógica RECALCULA todo desde (IdEmpleado, FechaSalida, MotivoSalida): lo que
                // el formulario diga de los montos no se mira. Solo se pasan OtrosMontos y la
                // observación.
                await _logica.Guardar(vm.IdEmpleado, vm.FechaSalida, vm.MotivoSalida,
                    vm.OtrosMontos, vm.Observacion,
                    ArmarContexto(AccionesAuditoria.CalcularLiquidacion));

                TempData["SuccessMessage"] =
                    "La liquidación se calculó y se guardó. El empleado quedó inactivo con su " +
                    "fecha y motivo de salida registrados.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await CargarCombos();
                return View(nameof(Create), vm);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await CargarCombos();
                return View(nameof(Create), vm);
            }
        }

        // Escenario 2: el desglose completo de una liquidación emitida.
        public async Task<IActionResult> Detalle(int id)
        {
            var desglose = await _logica.ObtenerDesglose(id);
            if (desglose == null) return NotFound();

            return View(desglose);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id, string motivo)
        {
            try
            {
                await _logica.Anular(id, motivo, ArmarContexto(AccionesAuditoria.AnularLiquidacion));

                TempData["SuccessMessage"] =
                    "La liquidación fue anulada y el empleado volvió a quedar activo, sin fecha " +
                    "ni motivo de salida.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                // Simetría con Calcular y Guardar: sin este catch, un InvalidOperationException
                // se escapaba al filtro global y terminaba en la pantalla amarilla de error.
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // La identidad viaja en el DTO: la Lógica y el Acceso a Datos no conocen Identity
        // ni HttpContext. IdUsuario queda NULL en la bitácora; el actor lo mete
        // ConstructorBitacora bajo "modificadoPor" dentro de ValorNuevo.
        //
        // El Email además es el puente hacia Liquidacion.IdUsuarioRegistro (FK a dbo.Usuario):
        // la Lógica lo resuelve por correo, igual que hace Factura con su emisor.
        private ContextoAuditoria ArmarContexto(string accion) => new()
        {
            UsuarioIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.Identity?.Name,
            Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Accion = accion
        };

        private async Task CargarCombos()
        {
            ViewBag.Empleados = await _logica.ObtenerEmpleadosLiquidables();
            ViewBag.Motivos = MotivosSalida.Todos;
        }
    }
}
