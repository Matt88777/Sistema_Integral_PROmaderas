using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos;
using PROmaderas.UI.Seguridad;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;

namespace PROmaderas.UI.Controllers
{
	[Authorize(Roles = "Administrador,Vendedor,Operador de Planta")]
	public class PedidosController : Controller
	{
		private readonly IProductoLogica _productoLogica;
		private readonly IWebHostEnvironment _env;
		private readonly Contexto _contexto;

		
		private readonly ApplicationDbContext _identityContext;

		public PedidosController(IProductoLogica productoLogica, IWebHostEnvironment env, Contexto contexto, ApplicationDbContext identityContext)
		{
			_productoLogica = productoLogica;
			_env = env;
			_contexto = contexto;
			_identityContext = identityContext;
		}

		public async Task<IActionResult> Index(string? filtroCliente, string? filtroEstado, int pagina = 1)
		{
			int registrosPorPagina = 10;
			var query = _contexto.Pedidos
				.Include(p => p.Cliente)
				.AsQueryable();

			if (!string.IsNullOrEmpty(filtroCliente))
				query = query.Where(p => p.Cliente!.Nombre.Contains(filtroCliente));

			if (!string.IsNullOrEmpty(filtroEstado))
				query = query.Where(p => p.Estado == filtroEstado);

			int totalRegistros = await query.CountAsync();

			var pedidos = await query
				.OrderByDescending(p => p.Fecha)
				.Skip((pagina - 1) * registrosPorPagina)
				.Take(registrosPorPagina)
				.ToListAsync();

			// esto trae solo los usuarios relacionados a los pedidos mostrados
			var usuarioIds = pedidos.Select(p => p.UsuarioId).Distinct().ToList();
			var usuariosDict = _identityContext.Users
								 .Where(u => usuarioIds.Contains(u.Id))
								 .ToDictionary(u => u.Id, u => u.Email);

			var pedidosDto = pedidos.Select(p => new PedidoListadoDto
			{
				Id = p.Id,
				Fecha = p.Fecha,
				Cliente = p.Cliente?.Nombre ?? "",
				Usuario = usuariosDict.ContainsKey(p.UsuarioId) ? usuariosDict[p.UsuarioId] : "Sin usuario",
				Subtotal = p.Subtotal,
				Impuestos = p.Impuestos,
				Total = p.Total,
				Estado = p.Estado
			}).ToList();

			ViewBag.FiltroCliente = filtroCliente;
			ViewBag.FiltroEstado = filtroEstado;
			ViewBag.PaginaActual = pagina;
			ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

			return View(pedidosDto);
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return NotFound();

			var pedido = await _contexto.Pedidos
				.Include(p => p.Cliente)
				.Include(p => p.Detalles)
					.ThenInclude(d => d.Producto)
				.FirstOrDefaultAsync(p => p.Id == id);

			if (pedido == null)
				return NotFound();

			// esto se hizo para tener el email del usuario que hizo el pedido, ya que en la tabla Pedido solo se guarda el Id del usuario
			var emailUsuario = _identityContext.Users
				.Where(u => u.Id == pedido.UsuarioId)
				.Select(u => u.Email)
				.FirstOrDefault() ?? pedido.UsuarioId; 

		
			ViewBag.EmailUsuario = emailUsuario;

			return View(pedido);

			
		}

		// Sprint 0 PROMADERAS: la creación/edición/eliminación de pedidos requiere
		// generar NumeroOrden (UNIQUE) y resolver IdVendedor (INT FK a Usuario)
		// que no están conectados al modelo actual. Quedan en construcción.

		private IActionResult PedidoEnConstruccion()
		{
			ViewBag.Modulo = "La creación, edición y eliminación de pedidos";
			ViewBag.Detalle = "PROMADERAS exige NumeroOrden e IdVendedor que aún no están conectados al modelo. Estará disponible en el próximo sprint.";
			return View("EnConstruccion");
		}

		[Authorize(Roles = "Administrador,Vendedor")]
		public IActionResult Create() => PedidoEnConstruccion();

		[HttpPost, ValidateAntiForgeryToken]
		[Authorize(Roles = "Administrador,Vendedor")]
		public IActionResult Create(ProductoAD producto, IFormFile? imagen) => PedidoEnConstruccion();

		[Authorize(Roles = Roles.Administrador)]
		public IActionResult Edit(int? id) => PedidoEnConstruccion();

		[HttpPost, ValidateAntiForgeryToken]
		[Authorize(Roles = Roles.Administrador)]
		public IActionResult Edit(int id, ProductoAD producto, IFormFile? imagen) => PedidoEnConstruccion();

		[Authorize(Roles = Roles.Administrador)]
		public IActionResult Delete(int? id) => PedidoEnConstruccion();

		[HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
		[Authorize(Roles = Roles.Administrador)]
		public IActionResult DeleteConfirmed(int id) => PedidoEnConstruccion();

		private async Task<PedidoAD?> ObtenerPedidoCompleto(int id)
		{
			return await _contexto.Pedidos
				.Include(p => p.Cliente)
				.Include(p => p.Detalles)
					.ThenInclude(d => d.Producto)
				.FirstOrDefaultAsync(p => p.Id == id);
		}

		public async Task<IActionResult> ExportarPdf(int id)
		{
			var pedido = await ObtenerPedidoCompleto(id);

			if (pedido == null)
				return NotFound();

			var emailUsuario = _identityContext.Users
				.Where(u => u.Id == pedido.UsuarioId)
				.Select(u => u.Email)
				.FirstOrDefault() ?? pedido.UsuarioId;

			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4);
					page.Margin(30);
					page.DefaultTextStyle(x => x.FontSize(11));

					page.Header()
						.Text($"Detalle del Pedido #{pedido.Id}")
						.FontSize(20)
						.Bold();

					page.Content().Column(col =>
					{
						col.Spacing(10);

						col.Item().Text($"Cliente: {pedido.Cliente?.Nombre}");
						col.Item().Text($"Cédula: {pedido.Cliente?.Cedula}");
						col.Item().Text($"Fecha: {pedido.Fecha:dd/MM/yyyy HH:mm}");
						col.Item().Text($"Usuario: {emailUsuario}");
						col.Item().Text($"Estado: {pedido.Estado}");

						col.Item().PaddingTop(10).Text("Productos del pedido").Bold();

						col.Item().Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn(3);
								columns.RelativeColumn(2);
								columns.RelativeColumn(1);
								columns.RelativeColumn(2);
								columns.RelativeColumn(1);
								columns.RelativeColumn(2);
							});

							table.Header(header =>
							{
								header.Cell().Element(CellStyle).Text("Producto").Bold();
								header.Cell().Element(CellStyle).Text("Precio Unitario").Bold();
								header.Cell().Element(CellStyle).Text("Cantidad").Bold();
								header.Cell().Element(CellStyle).Text("Descuento").Bold();
								header.Cell().Element(CellStyle).Text("Impuesto %").Bold();
								header.Cell().Element(CellStyle).Text("Total Línea").Bold();
							});

							foreach (var item in pedido.Detalles ?? Enumerable.Empty<PedidoDetalleAD>())
							{
								table.Cell().Element(CellStyle).Text(item.Producto?.Nombre ?? "");
								table.Cell().Element(CellStyle).Text(item.PrecioUnit.ToString("N2"));
								table.Cell().Element(CellStyle).Text(item.Cantidad.ToString());
								table.Cell().Element(CellStyle).Text(item.Descuento.ToString("N2"));
								table.Cell().Element(CellStyle).Text(item.ImpuestoPorc.ToString("N2"));
								table.Cell().Element(CellStyle).Text(item.TotalLinea.ToString("N2"));
							}
						});

						col.Item().PaddingTop(10).Text($"Subtotal: {pedido.Subtotal:N2}");
						col.Item().Text($"Impuestos: {pedido.Impuestos:N2}");
						col.Item().Text($"Total: {pedido.Total:N2}").Bold();
					});

					page.Footer()
						.AlignCenter()
						.Text($"PROmaderas - {DateTime.Now:dd/MM/yyyy HH:mm}");
				});
			});

			static IContainer CellStyle(IContainer container)
			{
				return container
					.Border(1)
					.BorderColor(Colors.Grey.Lighten2)
					.Padding(5);
			}

			var pdfBytes = document.GeneratePdf();

			return File(pdfBytes, "application/pdf", $"Pedido_{pedido.Id}.pdf");
		}

		public async Task<IActionResult> ExportarExcel(int id)
		{
			var pedido = await ObtenerPedidoCompleto(id);

			if (pedido == null)
				return NotFound();

			using var workbook = new XLWorkbook();
			var ws = workbook.Worksheets.Add($"Pedido_{pedido.Id}");

			ws.Cell(1, 1).Value = $"Detalle del Pedido #{pedido.Id}";
			ws.Cell(3, 1).Value = "Cliente";
			ws.Cell(3, 2).Value = pedido.Cliente?.Nombre ?? "";
			ws.Cell(4, 1).Value = "Cédula";
			ws.Cell(4, 2).Value = pedido.Cliente?.Cedula ?? "";
			ws.Cell(5, 1).Value = "Fecha";
			ws.Cell(5, 2).Value = pedido.Fecha.ToString("dd/MM/yyyy HH:mm");
			ws.Cell(6, 1).Value = "Estado";
			ws.Cell(6, 2).Value = pedido.Estado;

			ws.Cell(8, 1).Value = "Producto";
			ws.Cell(8, 2).Value = "Precio Unitario";
			ws.Cell(8, 3).Value = "Cantidad";
			ws.Cell(8, 4).Value = "Descuento";
			ws.Cell(8, 5).Value = "Impuesto %";
			ws.Cell(8, 6).Value = "Total Línea";

			int row = 9;
			foreach (var item in pedido.Detalles ?? Enumerable.Empty<PedidoDetalleAD>())
			{
				ws.Cell(row, 1).Value = item.Producto?.Nombre ?? "";
				ws.Cell(row, 2).Value = (double)item.PrecioUnit;
				ws.Cell(row, 3).Value = item.Cantidad;
				ws.Cell(row, 4).Value = (double)item.Descuento;
				ws.Cell(row, 5).Value = (double)item.ImpuestoPorc;
				ws.Cell(row, 6).Value = (double)item.TotalLinea;
				row++;
			}

			ws.Cell(row + 1, 5).Value = "Subtotal";
			ws.Cell(row + 1, 6).Value = (double)pedido.Subtotal;
			ws.Cell(row + 2, 5).Value = "Impuestos";
			ws.Cell(row + 2, 6).Value = (double)pedido.Impuestos;
			ws.Cell(row + 3, 5).Value = "Total";
			ws.Cell(row + 3, 6).Value = (double)pedido.Total;

			ws.Columns().AdjustToContents();

			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			stream.Position = 0;

			return File(
				stream.ToArray(),
				"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
				$"Pedido_{pedido.Id}.xlsx");
		}

		[Authorize]
		public IActionResult PedidoCreadoExitoso(int id)
		{
			TempData["Mensaje"] = "Pedido creado exitosamente.";
			return RedirectToAction(nameof(Details), new { id });
		}

		// Sprint 0: el CHECK constraint en OrdenCompra.Estado solo acepta
		// (Pendiente, En Produccion, Lista para Entrega, Entregada, Cancelada).
		// "Completado" no es valor válido, así que los flujos quedan en
		// construcción hasta que se ajuste el código al vocabulario de la BD.

		[HttpPost]
		[Authorize(Roles = Roles.Administrador)]
		public IActionResult CompletarPedido(int id) => PedidoEnConstruccion();

		[HttpPost]
		[Authorize(Roles = Roles.Administrador)]
		public IActionResult CancelarPedido(int id) => PedidoEnConstruccion();

	}
}