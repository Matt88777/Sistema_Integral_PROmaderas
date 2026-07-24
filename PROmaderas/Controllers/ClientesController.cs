using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.UI.Models;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = Roles.Administrador + "," + Roles.Gerente + "," + Roles.Vendedor)]
    public class ClientesController : Controller
    {
        private readonly IClienteLogica _clienteLogica;

        public ClientesController(IClienteLogica clienteLogica)
        {
            _clienteLogica = clienteLogica;
        }

        public async Task<IActionResult> Index(string? filtro, bool? filtroEstado, int pagina = 1)
        {
            int registrosPorPagina = 10;

            var (clientes, totalRegistros) = await _clienteLogica.ObtenerPaginado(
                pagina, registrosPorPagina, filtro, filtroEstado);

            ViewBag.Filtro = filtro;
            ViewBag.FiltroEstado = filtroEstado;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

            return View(clientes);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _clienteLogica.ObtenerPorId(id.Value);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClienteAD cliente)
        {
            if (!cliente.Exonerado)
                cliente.PorcentajeExoneracion = 0;

            ModelState.Remove(nameof(ClienteAD.PorcentajeExoneracion));

            if (ModelState.IsValid)
            {
                try
                {
                    await _clienteLogica.Crear(cliente);
                    TempData["Mensaje"] = "Cliente creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al crear el cliente: {ex.Message}");
                }
            }

            return View(cliente);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _clienteLogica.ObtenerPorId(id.Value);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClienteAD cliente)
        {

            if (!cliente.Exonerado)
                cliente.PorcentajeExoneracion = 0;

            ModelState.Remove(nameof(ClienteAD.PorcentajeExoneracion));
            {
                try
                {
                    var contextoAuditoria = new ContextoAuditoria
                    {
                        UsuarioIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        Email = User.Identity?.Name,
                        Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Accion = "Update"
                    };

                    await _clienteLogica.Actualizar(cliente, contextoAuditoria);
                    TempData["Mensaje"] = "Cliente actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al actualizar el cliente: {ex.Message}");
                }
            }

            return View(cliente);
        }

        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _clienteLogica.ObtenerPorId(id.Value);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador + "," + Roles.Gerente)]
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

                await _clienteLogica.CambiarEstado(id, contextoAuditoria);

                TempData["Mensaje"] = "El estado del cliente fue actualizado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al cambiar el estado del cliente.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _clienteLogica.Eliminar(id);
                TempData["Mensaje"] = "Cliente eliminado exitosamente";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al intentar eliminar el cliente.";
            }

            return RedirectToAction(nameof(Index));
        }
        //Historial del cliente
        public async Task<IActionResult> Historial(int id)
        {
            var (cliente, pedidos) = await _clienteLogica.ObtenerHistorialPorCliente(id);

            if (cliente == null)
                return NotFound();

            var vm = new ClienteHistorialViewModel
            {
                Cliente = cliente,
                Pedidos = pedidos.Select(p => new PedidoHistorialItemViewModel
                {
                    Id = p.Id,
                    NumeroOrden = p.NumeroOrden,
                    Fecha = p.Fecha,
                    Estado = p.Estado,
                    Subtotal = p.Subtotal,
                    Impuestos = p.Impuestos,
                    Total = p.Total,
                    Observacion = p.Observacion,
                    Detalles = p.Detalles?.ToList() ?? new(),
                    Factura = null
                }).ToList()
            };

            return View(vm);
        }
    }
}