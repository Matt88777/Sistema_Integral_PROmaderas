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
    [Authorize(Roles = Roles.Administrador + "," + Roles.Contador + "," + Roles.Gerente)]
    public class FacturacionController : Controller
    {
        private readonly IFacturacionLogica _logica;
        private readonly IClienteLogica _clienteLogica;
        public FacturacionController(IFacturacionLogica logica, IClienteLogica clienteLogica)
        {
            _logica = logica;
            _clienteLogica = clienteLogica;
        }

        public async Task<IActionResult> Index(int? clienteId, DateTime? fechaDesde,
                                               DateTime? fechaHasta, string? numeroFactura)
        {
            var vm = new ConsultaFacturasViewModel
            {
                ClienteId = clienteId,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                NumeroFactura = numeroFactura,
                Facturas = await _logica.BuscarConFiltros(clienteId, fechaDesde, fechaHasta, numeroFactura),
                Clientes = await _clienteLogica.ObtenerTodos()
            };
            return View(vm);
        }

        [Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
        public async Task<IActionResult> Create(int? pedidoId)
        {
            var ordenesDisponibles = await _logica.ObtenerOrdenesFacturables();

            ViewBag.OrdenesDisponibles = ordenesDisponibles;
            var vm = new CreateFacturaViewModel();

            if (pedidoId.HasValue)
            {
                var oc = ordenesDisponibles.FirstOrDefault(p => p.Id == pedidoId.Value);
                if (oc != null)
                {
                    var cli = oc.Cliente!;
                    decimal exo = oc.Subtotal * (cli.PorcentajeExoneracion / 100m);
                    decimal iva = Math.Round((oc.Subtotal - exo) * 0.13m, 2);
                    vm.PedidoId = oc.Id;
                    vm.NumeroOrden = oc.NumeroOrden;
                    vm.ClienteNombre = cli.Nombre;
                    vm.Exonerado = cli.Exonerado;
                    vm.PorcentajeExo = cli.PorcentajeExoneracion;
                    vm.SubtotalOC = oc.Subtotal;
                    vm.ExoneracionCalc = Math.Round(exo, 2);
                    vm.ImpuestoCalc = iva;
                    vm.TotalCalc = Math.Round(oc.Subtotal + iva, 2);
                }
            }
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
        public async Task<IActionResult> Create(CreateFacturaViewModel vm)
        {
            bool yaFacturada = await _logica.PedidoYaFacturado(vm.PedidoId);
            if (yaFacturada)
                ModelState.AddModelError("", "Esta orden ya tiene una factura emitida.");

            var oc = await _logica.ObtenerPedidoParaFacturar(vm.PedidoId);
            if (oc == null)
                ModelState.AddModelError("PedidoId", "La orden no existe.");

            if (!ModelState.IsValid)
            {
                ViewBag.OrdenesDisponibles = await _logica.ObtenerOrdenesFacturables();
                return View(vm);
            }

            var cli2 = oc!.Cliente!;
            decimal exo2 = cli2.Exonerado
                ? Math.Round(oc.Subtotal * (cli2.PorcentajeExoneracion / 100m), 2) : 0m;
            decimal iva2 = Math.Round((oc.Subtotal - exo2) * 0.13m, 2);

            var factura = new FacturacionAD
            {
                PedidoId = oc.Id,
                ClienteId = oc.ClienteId,
                Subtotal = oc.Subtotal,
                Exoneracion = exo2,
                Impuestos = iva2,
                Total = Math.Round(oc.Subtotal + iva2, 2)
            };

            // En este proyecto el UserName de Identity es el correo (mismo patrón que PedidosController).
            var correoEmisor = User.Identity?.Name ?? "";
            var creada = await _logica.Crear(factura, correoEmisor);
            TempData["Mensaje"] = $"Factura {creada.NumeroFactura} emitida exitosamente.";
            return RedirectToAction(nameof(Details), new { id = creada.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var factura = await _logica.ObtenerDetalle(id);
            if (factura == null) return NotFound();
            return View(factura);
        }

        // FAC-HU-003: el Contador cambia el estado de una factura (Emitida <-> Pendiente de Pago).
        [Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var factura = await _logica.ObtenerDetalle(id);
            if (factura == null) return NotFound();

            var vm = new CambiarEstadoFacturaViewModel
            {
                FacturaId = factura.Id,
                NumeroFactura = factura.NumeroFactura,
                EstadoActual = factura.Estado,
                NuevoEstado = factura.Estado
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
        public async Task<IActionResult> CambiarEstado(CambiarEstadoFacturaViewModel vm)
        {
            try
            {
                // La identidad viaja en el DTO (la Lógica/AD no conocen Identity ni HttpContext).
                var ctx = new ContextoAuditoria
                {
                    UsuarioIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Email = User.Identity?.Name,
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Accion = "Cambio de estado factura"
                };

                await _logica.CambiarEstado(vm.FacturaId, vm.NuevoEstado, ctx);
                TempData["Mensaje"] = "El estado de la factura fue actualizado correctamente.";
                return RedirectToAction(nameof(Details), new { id = vm.FacturaId });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);

                // Recargar el contexto de la factura para re-renderizar la vista.
                var factura = await _logica.ObtenerDetalle(vm.FacturaId);
                if (factura == null) return NotFound();
                vm.NumeroFactura = factura.NumeroFactura;
                vm.EstadoActual = factura.Estado;
                return View(vm);
            }
        }
    }
}
