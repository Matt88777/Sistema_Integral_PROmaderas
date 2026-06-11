using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos;
using PROmaderas.UI.Models;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = Roles.Administrador + "," + Roles.Contador + "," + Roles.Gerente)]
    public class FacturacionController : Controller
    {
        private readonly Contexto _contexto;
        public FacturacionController(Contexto contexto) { _contexto = contexto; }

        public async Task<IActionResult> Index()
        {
            var facturas = await _contexto.Facturaciones
                .Include(f => f.Pedido)
                .Include(f => f.Cliente)
                .Where(f => f.Activa)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();
            return View(facturas);
        }

        [Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
        public async Task<IActionResult> Create(int? pedidoId)
        {
            var idsYaFacturados = await _contexto.Facturaciones
                .Where(f => f.Activa).Select(f => f.PedidoId).ToListAsync();

            var ordenesDisponibles = await _contexto.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.Activa && p.Estado != "Cancelada"
                         && !idsYaFacturados.Contains(p.Id))
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

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
            bool yaFacturada = await _contexto.Facturaciones
                .AnyAsync(f => f.PedidoId == vm.PedidoId && f.Activa);
            if (yaFacturada)
                ModelState.AddModelError("", "Esta orden ya tiene una factura emitida.");

            var oc = await _contexto.Pedidos.Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Id == vm.PedidoId);
            if (oc == null)
                ModelState.AddModelError("PedidoId", "La orden no existe.");

            if (!ModelState.IsValid)
            {
                var ids = await _contexto.Facturaciones
                    .Where(f => f.Activa).Select(f => f.PedidoId).ToListAsync();
                ViewBag.OrdenesDisponibles = await _contexto.Pedidos
                    .Include(p => p.Cliente)
                    .Where(p => p.Activa && p.Estado != "Cancelada" && !ids.Contains(p.Id))
                    .OrderByDescending(p => p.Fecha).ToListAsync();
                return View(vm);
            }

            var cli2 = oc!.Cliente!;
            decimal exo2 = cli2.Exonerado
                ? Math.Round(oc.Subtotal * (cli2.PorcentajeExoneracion / 100m), 2) : 0m;
            decimal iva2 = Math.Round((oc.Subtotal - exo2) * 0.13m, 2);
            var ahora = DateTime.Now;
            var maxId = await _contexto.Facturaciones.MaxAsync(f => (int?)f.Id) ?? 0;

            var factura = new FacturacionAD
            {
                NumeroFactura = $"FAC-{ahora:yyyyMMdd}-{(maxId + 1):D4}",
                PedidoId = oc.Id,
                ClienteId = oc.ClienteId,
                Fecha = ahora,
                Subtotal = oc.Subtotal,
                Exoneracion = exo2,
                Impuestos = iva2,
                Total = Math.Round(oc.Subtotal + iva2, 2),
                Estado = "Emitida",
                Activa = true
            };

            _contexto.Facturaciones.Add(factura);
            await _contexto.SaveChangesAsync();
            TempData["Mensaje"] = $"Factura {factura.NumeroFactura} emitida exitosamente.";
            return RedirectToAction(nameof(Details), new { id = factura.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var factura = await _contexto.Facturaciones
                .Include(f => f.Pedido)
                    .ThenInclude(p => p!.Detalles!)
                        .ThenInclude(d => d.Producto)
                .Include(f => f.Cliente)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (factura == null) return NotFound();
            return View(factura);
        }
    }
}