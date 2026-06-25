using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos;
using PROmaderas.UI.Models;
using PROmaderas.UI.Seguridad;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;

namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = "Administrador,Gerente,Contador,Vendedor,Operador de Planta,Usuario")]
    public class PedidosController : Controller
    {
        private readonly IProductoLogica _productoLogica;
        private readonly IWebHostEnvironment _env;
        private readonly Contexto _contexto;

        private static readonly string[] EstadosValidos =
            { "Pendiente", "En Produccion", "Lista para Entrega", "Entregada", "Cancelada" };

        public PedidosController(
            IProductoLogica productoLogica,
            IWebHostEnvironment env,
            Contexto contexto)
        {
            _productoLogica = productoLogica;
            _env = env;
            _contexto = contexto;
        }

        // ── INDEX ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            string? filtroCliente, string? filtroEstado,
            string? fechaDesde, string? fechaHasta, int pagina = 1)
        {
            int registrosPorPagina = 10;

            var query = _contexto.Pedidos
                .Include(p => p.Cliente)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtroCliente))
                query = query.Where(p => p.Cliente!.Nombre.Contains(filtroCliente));

            if (!string.IsNullOrWhiteSpace(filtroEstado))
                query = query.Where(p => p.Estado == filtroEstado);

            if (DateTime.TryParse(fechaDesde, out var desde))
                query = query.Where(p => p.Fecha >= desde);

            if (DateTime.TryParse(fechaHasta, out var hasta))
                query = query.Where(p => p.Fecha <= hasta.AddDays(1));

            int totalRegistros = await query.CountAsync();
            var pedidos = await query
                .OrderByDescending(p => p.Fecha)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            var vendedorIds = pedidos.Select(p => p.VendedorId).Distinct().ToList();
            var vendedoresDict = await _contexto.Usuarios
                .Where(u => vendedorIds.Contains(u.IdUsuario))
                .ToDictionaryAsync(u => u.IdUsuario, u => u.Correo);

            var pedidosDto = pedidos.Select(p => new PedidoListadoDto
            {
                Id = p.Id,
                NumeroOrden = p.NumeroOrden,
                Fecha = p.Fecha,
                Cliente = p.Cliente?.Nombre ?? "",
                Usuario = vendedoresDict.TryGetValue(p.VendedorId, out var v) ? v : "Sin vendedor",
                Subtotal = p.Subtotal,
                Impuestos = p.Impuestos,
                Total = p.Total,
				Estado = p.Estado,
				Activa = p.Activa
			}).ToList();

            ViewBag.FiltroCliente = filtroCliente;
            ViewBag.FiltroEstado = filtroEstado;
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);
            ViewBag.EstadosValidos = EstadosValidos;

            return View(pedidosDto);
        }

        // ── DETAILS ────────────────────────────────────────────────────────
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await _contexto.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles!)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            var vendedor = await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == pedido.VendedorId);

			ViewBag.NombreVendedor = vendedor?.Correo ?? $"Usuario #{pedido.VendedorId}";
			ViewBag.EstadosValidos = EstadosValidos;

			ViewBag.PuedeCambiarEstado = User.IsInRole(Roles.Administrador) ||
										 User.IsInRole(Roles.OperadorDePlanta);

			ViewBag.PuedeCancelar = User.IsInRole(Roles.Administrador) ||
									User.IsInRole(Roles.Gerente);

			ViewBag.PuedeRegistrarSalida = User.IsInRole(Roles.Administrador) ||
										   User.IsInRole(Roles.OperadorDePlanta);

			ViewBag.SalidaRegistrada = await _contexto.InventarioMovimientos
				.AnyAsync(m => m.IdOrdenCompra == pedido.Id && m.TipoMovimiento == "Salida");


			// OC-HU-004: cargar historial de cambios de estado
			var historial = new List<HistorialItemDto>();
            try
            {
                var conn = _contexto.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT h.FechaCambio, h.EstadoAnterior, h.EstadoNuevo, " +
                    "ISNULL(h.Observacion,'') AS Observacion, " +
                    "ISNULL(u.Correo,'Sistema') AS UsuarioCambio " +
                    "FROM HistorialEstadoOrden h " +
                    "LEFT JOIN Usuario u ON u.IdUsuario = h.IdUsuarioCambio " +
                    "WHERE h.IdOrdenCompra = @id " +
                    "ORDER BY h.FechaCambio DESC";
                var param = cmd.CreateParameter();
                param.ParameterName = "@id";
                param.Value = id;
                cmd.Parameters.Add(param);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    historial.Add(new HistorialItemDto
                    {
                        FechaCambio = reader.GetDateTime(0),
                        EstadoAnterior = reader.GetString(1),
                        EstadoNuevo = reader.GetString(2),
                        Observacion = reader.GetString(3),
                        UsuarioCambio = reader.GetString(4)
                    });
                }
            }
            catch { }
            ViewBag.Historial = historial;

            return View(pedido); 
        }

        // ── CREATE GET ─────────────────────────────────────────────────────
        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        public async Task<IActionResult> Create()
        {
            await CargarViewBagCreate();
            ViewBag.VendedorEmail = User.Identity?.Name ?? "(usuario no identificado)";
            return View(new CreatePedidoViewModel());
        }

        // ── CREATE POST ────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        public async Task<IActionResult> Create(CreatePedidoViewModel vm)
        {
            if (!vm.Detalles.Any())
                ModelState.AddModelError("", "Debe agregar al menos un producto al pedido.");

            if (!ModelState.IsValid)
            {
                await CargarViewBagCreate();
                return View(vm);
            }

            // Buscar IdVendedor en tabla Usuario por correo del usuario logueado
            var emailActual = User.Identity!.Name ?? "";
            var vendedor = await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == emailActual && u.Estado);

            if (vendedor == null)
            {
                ModelState.AddModelError("",
                    $"El usuario '{emailActual}' no existe en la tabla Usuario. " +
                    "Verifique que el empleado/vendedor esté registrado en el sistema interno.");
                await CargarViewBagCreate();
                return View(vm);
            }

            // Validar stock disponible (OC-HU-002)
            // Solo bloquea si hay datos en InventarioMovimiento Y el stock es insuficiente.
            // Si la tabla está vacía se permite la creación (sin movimientos = sin datos aún).
            var stockDict = await ObtenerStockDisponible();
            if (stockDict.Any())
            {
                var erroresStock = new List<string>();
                foreach (var det in vm.Detalles)
                {
                    int stockDisp = stockDict.TryGetValue(det.ProductoId, out var s) ? s : 0;
                    if (stockDisp > 0 && det.Cantidad > stockDisp)
                    {
                        var prod = await _contexto.Productos.FindAsync(det.ProductoId);
                        erroresStock.Add(
                            $"'{prod?.Nombre ?? det.ProductoId.ToString()}': " +
                            $"solicitado {det.Cantidad}, disponible {stockDisp}");
                    }
                }
                if (erroresStock.Any())
                {
                    ModelState.AddModelError("",
                        "Stock insuficiente: " + string.Join(" | ", erroresStock));
                    await CargarViewBagCreate();
                    return View(vm);
                }
            }

            // Generar NumeroOrden único: OC-YYYYMMDD-XXXX
            var ahora = DateTime.Now;
            var maxId = await _contexto.Pedidos.MaxAsync(p => (int?)p.Id) ?? 0;
            var numeroOrden = $"OC-{ahora:yyyyMMdd}-{(maxId + 1):D4}";

            // Calcular totales (IVA 13%)
            decimal subtotal = vm.Detalles.Sum(d => d.Cantidad * d.PrecioUnit);
            decimal impuesto = Math.Round(subtotal * 0.13m, 2);
            decimal total = subtotal + impuesto;

            var pedido = new PedidoAD
            {
                NumeroOrden = numeroOrden,
                ClienteId = vm.ClienteId,
                VendedorId = vendedor.IdUsuario,
                Fecha = ahora,
                Observacion = vm.Observacion,
                Subtotal = Math.Round(subtotal, 2),
                Impuestos = impuesto,
                Total = Math.Round(total, 2),
                Estado = "Pendiente",
                Activa = true
            };

            _contexto.Pedidos.Add(pedido);
            await _contexto.SaveChangesAsync();

            foreach (var det in vm.Detalles)
            {
                _contexto.PedidoDetalles.Add(new PedidoDetalleAD
                {
                    PedidoId = pedido.Id,
                    ProductoId = det.ProductoId,
                    Cantidad = det.Cantidad,
                    PrecioUnit = det.PrecioUnit,
                    TotalLinea = Math.Round(det.Cantidad * det.PrecioUnit, 2)
                });
            }
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = $"Pedido {numeroOrden} creado exitosamente.";
            return RedirectToAction(nameof(Details), new { id = pedido.Id });
        }

		// ── CAMBIAR ESTADO (OC-HU-004) ─────────────────────────────────────

		// ── CAMBIAR ESTADO (OC-HU-004) ─────────────────────────────────────
		[HttpPost, ValidateAntiForgeryToken]
		[Authorize(Roles = Roles.Administrador + "," + Roles.OperadorDePlanta)]
		public async Task<IActionResult> CambiarEstado(
			int id, string nuevoEstado, string? observacion)
		{
			if (!EstadosValidos.Contains(nuevoEstado))
			{
				TempData["Error"] = "Estado no válido.";
				return RedirectToAction(nameof(Details), new { id });
			}

			var pedido = await _contexto.Pedidos.FindAsync(id);
			if (pedido == null) return NotFound();

			if (!pedido.Activa || pedido.Estado == "Cancelada")
			{
				TempData["Error"] = "No se puede cambiar el estado de una orden cancelada o inactiva.";
				return RedirectToAction(nameof(Details), new { id });
			}

			if (pedido.Estado == nuevoEstado)
			{
				TempData["Error"] = "La orden ya se encuentra en ese estado.";
				return RedirectToAction(nameof(Details), new { id });
			}

			if (nuevoEstado == "Cancelada" &&
				!(User.IsInRole(Roles.Administrador) || User.IsInRole(Roles.Gerente)))
			{
				TempData["Error"] = "Solo el Gerente o Administrador puede cancelar una orden.";
				return RedirectToAction(nameof(Details), new { id });
			}

			var estadoAnterior = pedido.Estado;

			pedido.Estado = nuevoEstado;

			if (nuevoEstado == "Cancelada")
			{
				pedido.Activa = false;
			}

			await _contexto.SaveChangesAsync();

			await RegistrarHistorialEstadoOrdenAsync(
				pedido.Id,
				estadoAnterior,
				nuevoEstado,
				observacion
			);

			TempData["Mensaje"] =
				$"Estado actualizado: '{estadoAnterior}' → '{nuevoEstado}'.";

			return RedirectToAction(nameof(Details), new { id });
		}


		// ── CANCELAR ORDEN (OC-HU-005) ─────────────────────────────────────
		[HttpPost, ValidateAntiForgeryToken]
		[Authorize(Roles = Roles.Administrador + "," + Roles.Gerente)]
		public async Task<IActionResult> Cancelar(int id, string? motivo)
		{
			var pedido = await _contexto.Pedidos
				.Include(p => p.Detalles)
				.FirstOrDefaultAsync(p => p.Id == id);

			if (pedido == null)
			{
				return NotFound();
			}

			if (!pedido.Activa || pedido.Estado == "Cancelada")
			{
				TempData["Error"] = "La orden ya se encuentra cancelada.";
				return RedirectToAction(nameof(Details), new { id });
			}

			if (pedido.Estado == "Entregada")
			{
				TempData["Error"] = "No se puede cancelar una orden que ya fue entregada.";
				return RedirectToAction(nameof(Details), new { id });
			}

			var estadoAnterior = pedido.Estado;

			pedido.Estado = "Cancelada";
			pedido.Activa = false;

			await _contexto.SaveChangesAsync();

			var observacionHistorial = string.IsNullOrWhiteSpace(motivo)
				? "Orden cancelada."
				: motivo.Trim();

			await RegistrarHistorialEstadoOrdenAsync(
				pedido.Id,
				estadoAnterior,
				"Cancelada",
				observacionHistorial
			);

			TempData["Mensaje"] =
				$"Orden {pedido.NumeroOrden} cancelada correctamente. El historial y detalle se conservaron.";

			return RedirectToAction(nameof(Details), new { id = pedido.Id });
		}

		// ── REGISTRAR SALIDA DE INVENTARIO (INV-HU-005) ─────────────────────
		[HttpPost, ValidateAntiForgeryToken]
		[Authorize(Roles = Roles.Administrador + "," + Roles.OperadorDePlanta)]
		public async Task<IActionResult> RegistrarSalidaInventario(int id)
		{
			var pedido = await _contexto.Pedidos
				.Include(p => p.Detalles!)
					.ThenInclude(d => d.Producto)
				.FirstOrDefaultAsync(p => p.Id == id);

			if (pedido == null)
			{
				return NotFound();
			}

			if (!pedido.Activa || pedido.Estado == "Cancelada")
			{
				TempData["Error"] = "No se puede registrar salida de inventario para una orden cancelada o inactiva.";
				return RedirectToAction(nameof(Details), new { id });
			}

			if (pedido.Estado == "Entregada")
			{
				TempData["Error"] = "La orden ya fue entregada.";
				return RedirectToAction(nameof(Details), new { id });
			}

			if (pedido.Detalles == null || !pedido.Detalles.Any())
			{
				TempData["Error"] = "La orden no tiene productos asociados.";
				return RedirectToAction(nameof(Details), new { id });
			}

			var salidaYaRegistrada = await _contexto.InventarioMovimientos
				.AnyAsync(m => m.IdOrdenCompra == pedido.Id && m.TipoMovimiento == "Salida");

			if (salidaYaRegistrada)
			{
				TempData["Error"] = "Esta orden ya tiene una salida de inventario registrada.";
				return RedirectToAction(nameof(Details), new { id });
			}

			var idUsuarioRegistro = await ObtenerIdUsuarioActualAsync();

			if (!idUsuarioRegistro.HasValue)
			{
				TempData["Error"] = "No se pudo identificar el usuario que registra la salida.";
				return RedirectToAction(nameof(Details), new { id });
			}

			var productosSolicitados = pedido.Detalles
				.GroupBy(d => d.ProductoId)
				.Select(g => new
				{
					IdTipoTarima = g.Key,
					Nombre = g.First().Producto?.Nombre ?? $"Producto #{g.Key}",
					Cantidad = g.Sum(x => x.Cantidad)
				})
				.ToList();

			var stockDisponible = await ObtenerStockDisponible();
			var erroresInventario = new List<string>();

			foreach (var item in productosSolicitados)
			{
				int disponible = stockDisponible.TryGetValue(item.IdTipoTarima, out var stock)
					? stock
					: 0;

				if (disponible < item.Cantidad)
				{
					erroresInventario.Add(
						$"{item.Nombre}: disponible {disponible}, requerido {item.Cantidad}");
				}
			}

			if (erroresInventario.Any())
			{
				TempData["Error"] = "Inventario insuficiente. " + string.Join(" | ", erroresInventario);
				return RedirectToAction(nameof(Details), new { id });
			}

			using var transaccion = await _contexto.Database.BeginTransactionAsync();

			try
			{
				foreach (var item in productosSolicitados)
				{
					_contexto.InventarioMovimientos.Add(new InventarioMovimientoAD
					{
						IdTipoTarima = item.IdTipoTarima,
						IdUsuarioRegistro = idUsuarioRegistro.Value,
						TipoMovimiento = "Salida",
						Cantidad = item.Cantidad,
						FechaMovimiento = DateTime.Now,
						Motivo = $"Salida por despacho de orden {pedido.NumeroOrden}",
						IdOrdenCompra = pedido.Id
					});
				}

				var estadoAnterior = pedido.Estado;

				pedido.Estado = "Entregada";

				await _contexto.SaveChangesAsync();

				await RegistrarHistorialEstadoOrdenAsync(
					pedido.Id,
					estadoAnterior,
					"Entregada",
					"Salida de inventario registrada al despachar la orden."
				);

				await transaccion.CommitAsync();

				TempData["Mensaje"] =
					$"Salida de inventario registrada correctamente para la orden {pedido.NumeroOrden}.";
			}
			catch
			{
				await transaccion.RollbackAsync();
				TempData["Error"] = "Ocurrió un error al registrar la salida de inventario.";
			}

			return RedirectToAction(nameof(Details), new { id });
		}

		// ── REGISTRAR HISTORIAL DE ESTADO ──────────────────────────────────
		private async Task RegistrarHistorialEstadoOrdenAsync(
			int idOrdenCompra,
			string? estadoAnterior,
			string estadoNuevo,
			string? observacion)
		{
			var emailActual = User.Identity?.Name ?? string.Empty;

			int? idUsuarioCambio = await _contexto.Usuarios
				.Where(u => u.Correo == emailActual)
				.Select(u => (int?)u.IdUsuario)
				.FirstOrDefaultAsync();

			var observacionFinal = observacion;

			if (!idUsuarioCambio.HasValue)
			{
				idUsuarioCambio = await _contexto.Usuarios
					.OrderBy(u => u.IdUsuario)
					.Select(u => (int?)u.IdUsuario)
					.FirstOrDefaultAsync();

				observacionFinal = string.IsNullOrWhiteSpace(observacionFinal)
					? $"Cambio realizado por usuario Identity: {emailActual}"
					: $"{observacionFinal} | Usuario Identity: {emailActual}";
			}

			if (!idUsuarioCambio.HasValue)
			{
				return;
			}

			await _contexto.Database.ExecuteSqlRawAsync(
				@"INSERT INTO HistorialEstadoOrden
            (IdOrdenCompra, EstadoAnterior, EstadoNuevo, IdUsuarioCambio, FechaCambio, Observacion)
          VALUES ({0}, {1}, {2}, {3}, GETDATE(), {4})",
				idOrdenCompra,
				(object?)estadoAnterior ?? DBNull.Value,
				estadoNuevo,
				idUsuarioCambio.Value,
				(object?)observacionFinal ?? DBNull.Value);
		}

		// ── EXPORTAR PDF ───────────────────────────────────────────────────
		public async Task<IActionResult> ExportarPdf(int id)
        {
            var pedido = await ObtenerPedidoCompleto(id);
            if (pedido == null) return NotFound();

            var vendedor = await _contexto.Usuarios.FindAsync(pedido.VendedorId);
            var emailVendedor = vendedor?.Correo ?? $"Usuario #{pedido.VendedorId}";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text($"Orden de Compra — {pedido.NumeroOrden}")
                        .FontSize(18).Bold();

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Text($"Cliente: {pedido.Cliente?.Nombre}");
                        col.Item().Text($"Cédula: {pedido.Cliente?.Cedula}");
                        col.Item().Text($"Fecha: {pedido.Fecha:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Vendedor: {emailVendedor}");
                        col.Item().Text($"Estado: {pedido.Estado}");
                        if (!string.IsNullOrEmpty(pedido.Observacion))
                            col.Item().Text($"Observación: {pedido.Observacion}");

                        col.Item().PaddingTop(10).Text("Detalle de productos").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Element(CellStyle).Text("Producto").Bold();
                                h.Cell().Element(CellStyle).Text("Precio Unit.").Bold();
                                h.Cell().Element(CellStyle).Text("Cant.").Bold();
                                h.Cell().Element(CellStyle).Text("Subtotal").Bold();
                            });
                            foreach (var item in pedido.Detalles ?? Enumerable.Empty<PedidoDetalleAD>())
                            {
                                table.Cell().Element(CellStyle).Text(item.Producto?.Nombre ?? "");
                                table.Cell().Element(CellStyle).Text($"₡{item.PrecioUnit:N2}");
                                table.Cell().Element(CellStyle).Text(item.Cantidad.ToString());
                                table.Cell().Element(CellStyle).Text($"₡{item.TotalLinea:N2}");
                            }
                        });

                        col.Item().PaddingTop(8).Text($"Subtotal: ₡{pedido.Subtotal:N2}");
                        col.Item().Text($"Impuesto (13%): ₡{pedido.Impuestos:N2}");
                        col.Item().Text($"TOTAL: ₡{pedido.Total:N2}").Bold();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"PROmaderas — {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });

            static IContainer CellStyle(IContainer c) =>
                c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5);

            return File(
                document.GeneratePdf(),
                "application/pdf",
                $"Pedido_{pedido.NumeroOrden}.pdf");
        }

        // ── EXPORTAR EXCEL ─────────────────────────────────────────────────
        public async Task<IActionResult> ExportarExcel(int id)
        {
            var pedido = await ObtenerPedidoCompleto(id);
            if (pedido == null) return NotFound();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Pedido");
            ws.Cell(1, 1).Value = $"Orden: {pedido.NumeroOrden}";
            ws.Cell(3, 1).Value = "Cliente"; ws.Cell(3, 2).Value = pedido.Cliente?.Nombre ?? "";
            ws.Cell(4, 1).Value = "Cédula"; ws.Cell(4, 2).Value = pedido.Cliente?.Cedula ?? "";
            ws.Cell(5, 1).Value = "Fecha"; ws.Cell(5, 2).Value = pedido.Fecha.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(6, 1).Value = "Estado"; ws.Cell(6, 2).Value = pedido.Estado;
            ws.Cell(8, 1).Value = "Producto"; ws.Cell(8, 2).Value = "Precio Unit.";
            ws.Cell(8, 3).Value = "Cantidad"; ws.Cell(8, 4).Value = "Subtotal";
            int row = 9;
            foreach (var item in pedido.Detalles ?? Enumerable.Empty<PedidoDetalleAD>())
            {
                ws.Cell(row, 1).Value = item.Producto?.Nombre ?? "";
                ws.Cell(row, 2).Value = (double)item.PrecioUnit;
                ws.Cell(row, 3).Value = item.Cantidad;
                ws.Cell(row, 4).Value = (double)item.TotalLinea;
                row++;
            }
            ws.Cell(row + 1, 3).Value = "Subtotal";
            ws.Cell(row + 1, 4).Value = (double)pedido.Subtotal;
            ws.Cell(row + 2, 3).Value = "Impuesto";
            ws.Cell(row + 2, 4).Value = (double)pedido.Impuestos;
            ws.Cell(row + 3, 3).Value = "Total";
            ws.Cell(row + 3, 4).Value = (double)pedido.Total;
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Pedido_{pedido.NumeroOrden}.xlsx");
        }

        // ── HELPERS PRIVADOS ───────────────────────────────────────────────
        private async Task<PedidoAD?> ObtenerPedidoCompleto(int id) =>
            await _contexto.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles!).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);

        private async Task CargarViewBagCreate()
        {
            var clientes = await _contexto.Clientes
                .Where(c => c.Estado == true)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var productos = await _contexto.Productos
                .Where(p => p.Activo == true)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.ClienteSelectList = new SelectList(clientes, "Id", "Nombre");
            ViewBag.ClientesJson = System.Text.Json.JsonSerializer.Serialize(
                clientes.Select(c => new { id = c.Id, nombre = c.Nombre }));
            ViewBag.ProductosJson = System.Text.Json.JsonSerializer.Serialize(
                productos.Select(p => new { id = p.Id, nombre = p.Nombre, precio = p.Precio }));
            ViewBag.StockDict = await ObtenerStockDisponible();
        }

		private async Task<int?> ObtenerIdUsuarioActualAsync()
		{
			var emailActual = User.Identity?.Name ?? string.Empty;

			var idUsuario = await _contexto.Usuarios
				.Where(u => u.Correo == emailActual)
				.Select(u => (int?)u.IdUsuario)
				.FirstOrDefaultAsync();

			if (!idUsuario.HasValue)
			{
				idUsuario = await _contexto.Usuarios
					.OrderBy(u => u.IdUsuario)
					.Select(u => (int?)u.IdUsuario)
					.FirstOrDefaultAsync();
			}

			return idUsuario;
		}

		private async Task<Dictionary<int, int>> ObtenerStockDisponible()
        {
            var stock = new Dictionary<int, int>();
            var conn = _contexto.Database.GetDbConnection();
            bool wasOpen = conn.State == System.Data.ConnectionState.Open;
            if (!wasOpen) await conn.OpenAsync();
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT IdTipoTarima,
                        SUM(CASE WHEN TipoMovimiento IN ('Entrada','AjusteEntrada')
                                 THEN Cantidad ELSE 0 END) -
                        SUM(CASE WHEN TipoMovimiento IN ('Salida','AjusteSalida')
                                 THEN Cantidad ELSE 0 END) AS StockDisponible
                    FROM InventarioMovimiento
                    GROUP BY IdTipoTarima";
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    stock[Convert.ToInt32(reader["IdTipoTarima"])] =
                        Convert.ToInt32(reader["StockDisponible"]);
            }
            finally
            {
                if (!wasOpen) await conn.CloseAsync();
            }
            return stock;
        }
    }
}
