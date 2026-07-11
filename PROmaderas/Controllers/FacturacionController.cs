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
                                               DateTime? fechaHasta, string? numeroFactura,
                                               bool incluirInactivas = false)
        {
            // FAC-HU-005: ver las inactivas es privilegio del Administrador. No alcanza con
            // ocultar el checkbox: cualquiera podría mandar ?incluirInactivas=true a mano.
            var incluir = incluirInactivas && User.IsInRole(Roles.Administrador);

            var vm = new ConsultaFacturasViewModel
            {
                ClienteId = clienteId,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                NumeroFactura = numeroFactura,
                IncluirInactivas = incluir,
                Facturas = await _logica.BuscarConFiltros(clienteId, fechaDesde, fechaHasta,
                                                          numeroFactura, incluir),
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
            // Historial de pagos vía ViewBag para no cambiar el @model FacturacionAD de la vista.
            ViewBag.Pagos = await _logica.ObtenerPagosPorFactura(id);

            // FAC-HU-005: el Details abre facturas anuladas y muestra por qué se anularon.
            if (!factura.Activa)
                ViewBag.MotivoAnulacion = await _logica.ObtenerMotivoAnulacion(id);

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

        // FAC-HU-004: el Contador registra un pago (parcial o total) de una factura.
        [Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
        public async Task<IActionResult> RegistrarPago(int id)
        {
            var factura = await _logica.ObtenerDetalle(id);
            if (factura == null) return NotFound();

            if (!factura.Activa || factura.Estado == EstadosFactura.Pagada || factura.SaldoPendiente <= 0)
            {
                TempData["Error"] = "Esta factura no admite registro de pagos (inactiva o ya pagada).";
                return RedirectToAction(nameof(Details), new { id });
            }

            var vm = new RegistrarPagoViewModel
            {
                FacturaId = factura.Id,
                NumeroFactura = factura.NumeroFactura,
                SaldoPendiente = factura.SaldoPendiente
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
        public async Task<IActionResult> RegistrarPago(RegistrarPagoViewModel vm)
        {
            try
            {
                // La identidad viaja en el DTO / por parámetro (Lógica y AD no conocen Identity).
                var ctx = new ContextoAuditoria
                {
                    UsuarioIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Email = User.Identity?.Name,
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Accion = "Registro de pago factura"
                };
                var correoOperador = User.Identity?.Name ?? "";

                await _logica.RegistrarPago(vm.FacturaId, vm.Monto, vm.FormaPago, vm.Referencia,
                    vm.FechaPago, correoOperador, ctx);

                TempData["Mensaje"] = "Pago registrado correctamente.";
                return RedirectToAction(nameof(Details), new { id = vm.FacturaId });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);

                // Recargar saldo/contexto para re-renderizar la vista.
                var factura = await _logica.ObtenerDetalle(vm.FacturaId);
                if (factura == null) return NotFound();
                vm.NumeroFactura = factura.NumeroFactura;
                vm.SaldoPendiente = factura.SaldoPendiente;
                return View(vm);
            }
        }

        // FAC-HU-005: el ADMINISTRADOR (no el Contador) inactiva/reactiva una factura.
        // Una sola pantalla: la dirección del cambio la marca el estado actual de Activa.
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> CambiarActiva(int id)
        {
            var factura = await _logica.ObtenerDetalle(id);
            if (factura == null) return NotFound();

            return View(await ArmarCambiarActivaVm(factura, new CambiarActivaFacturaViewModel()));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> CambiarActiva(CambiarActivaFacturaViewModel vm)
        {
            var factura = await _logica.ObtenerDetalle(vm.FacturaId);
            if (factura == null) return NotFound();

            if (!ModelState.IsValid)
                return View(await ArmarCambiarActivaVm(factura, vm));

            try
            {
                // La dirección del cambio sale de la BD, no del hidden del formulario.
                var inactivar = factura.Activa;

                // La identidad viaja en el DTO (la Lógica/AD no conocen Identity ni HttpContext).
                var ctx = new ContextoAuditoria
                {
                    UsuarioIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Email = User.Identity?.Name,
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Accion = inactivar
                        ? AccionesAuditoria.InactivarFactura
                        : AccionesAuditoria.ReactivarFactura
                };

                if (inactivar)
                {
                    await _logica.Inactivar(vm.FacturaId, vm.Motivo, ctx);
                    TempData["Mensaje"] = "La factura fue inactivada y quedó en estado Anulada.";
                }
                else
                {
                    await _logica.Reactivar(vm.FacturaId, vm.Motivo, ctx);
                    TempData["Mensaje"] = "La factura fue reactivada y su estado se recalculó.";
                }

                return RedirectToAction(nameof(Details), new { id = vm.FacturaId });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(await ArmarCambiarActivaVm(factura, vm));
            }
        }

        // Completa el contexto de la pantalla (estado, pagos, advertencia) desde la BD,
        // conservando lo único que el usuario escribe: el motivo.
        private async Task<CambiarActivaFacturaViewModel> ArmarCambiarActivaVm(
            FacturacionAD factura, CambiarActivaFacturaViewModel vm)
        {
            var pagos = await _logica.ObtenerPagosPorFactura(factura.Id);

            vm.FacturaId = factura.Id;
            vm.NumeroFactura = factura.NumeroFactura;
            vm.EstadoActual = factura.Estado;
            vm.ActivaActual = factura.Activa;
            vm.SaldoPendiente = factura.SaldoPendiente;
            vm.CantidadPagos = pagos.Count;
            vm.TotalPagado = pagos.Sum(p => p.Monto);

            // Solo aplica al flujo de reactivar: si la orden ya tiene otra factura activa,
            // la vista lo avisa y no deja confirmar (la Lógica igual lo rechaza en el POST).
            vm.ImpedimentoReactivacion = factura.Activa
                ? null
                : await _logica.ObtenerImpedimentoReactivacion(factura.Id);

            return vm;
        }
    }
}
