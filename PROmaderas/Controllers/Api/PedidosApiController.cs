using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROmaderas.AccesoADatos;
using PROmaderas.Abstracciones.Models;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers.Api
{
	[ApiController]
	[Route("api/pedidos")]
	[Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]

	public class PedidosApiController : ControllerBase
	{
		private readonly Contexto _contexto;
		private readonly IProductoLogica _productoLogica;

		public PedidosApiController(Contexto contexto, IProductoLogica productoLogica)
		{
			_contexto = contexto;
			_productoLogica = productoLogica;
		}

		public class LineaCalculoDto
		{
			public int ProductoId { get; set; }
			public int Cantidad { get; set; }
			public decimal Descuento { get; set; } = 0; 
			public string? DescuentoTipo { get; set; } 
		}

		public class ResultadoCalculoDto
		{
			public decimal Subtotal { get; set; }
			public decimal Impuestos { get; set; }
			public decimal Total { get; set; }
		}

		
		[HttpPost("calcular")]
		public async Task<IActionResult> Calcular([FromBody] List<LineaCalculoDto> lineas)
		{
			if (lineas == null || !lineas.Any())
				return BadRequest(new { error = "Se requieren líneas para calcular" });

			decimal subtotal = 0m;
			decimal impuestos = 0m;
			decimal total = 0m;

			foreach (var linea in lineas)
			{
				var producto = await _productoLogica.ObtenerPorId(linea.ProductoId);
				if (producto == null)
					return BadRequest(new { error = $"Producto {linea.ProductoId} no encontrado" });

				var precioUnit = producto.Precio;
				var impuestoPorc = producto.ImpuestoPorc;
				var cantidad = linea.Cantidad;
				if (cantidad <= 0) return BadRequest(new { error = "La cantidad debe ser mayor a 0" });

				var importe = precioUnit * cantidad;

				decimal descuentoAmount = 0m;
				var tipo = (linea.DescuentoTipo ?? "porc").ToLowerInvariant();
				if (tipo == "porc")
				{
					descuentoAmount = importe * (linea.Descuento / 100m);
				}
				else
				{
					descuentoAmount = linea.Descuento;
				}

				if (descuentoAmount < 0) descuentoAmount = 0;
				if (descuentoAmount > importe) descuentoAmount = importe;

				var baseGravable = importe - descuentoAmount;
				var impuestoLinea = baseGravable * (impuestoPorc / 100m);
				var totalLinea = baseGravable + impuestoLinea;

				subtotal += baseGravable;
				impuestos += impuestoLinea;
				total += totalLinea;
			}

			var resultado = new ResultadoCalculoDto
			{
				Subtotal = Math.Round(subtotal, 2),
				Impuestos = Math.Round(impuestos, 2),
				Total = Math.Round(total, 2)
			};

			return Ok(resultado);
		}

	
		public class CrearPedidoDto
		{
			public int ClienteId { get; set; }
			public List<LineaCalculoDto> Lineas { get; set; } = new();
		}

		public class ResultadoCrearPedidoDto
		{
			public int PedidoId { get; set; }
			public decimal Subtotal { get; set; }
			public decimal Impuestos { get; set; }
			public decimal Total { get; set; }
		}

		[HttpPost]
		public Task<IActionResult> Crear([FromBody] CrearPedidoDto dto)
		{
			// Sprint 0 PROMADERAS: la creación de pedidos requiere NumeroOrden e
			// IdVendedor (FK a Usuario) que no están conectados al modelo.
			// Endpoint deshabilitado temporalmente. El flujo completo se
			// implementará en el siguiente sprint.
			_ = dto;
			IActionResult resultado = StatusCode(503, new
			{
				error = "Crear pedidos está deshabilitado en Sprint 0",
				detalle = "PROMADERAS exige NumeroOrden e IdVendedor que aún no están conectados al modelo."
			});
			return Task.FromResult(resultado);

			/* IMPLEMENTACION ORIGINAL — comentada hasta el próximo sprint.
			if (dto == null || dto.Lineas == null || !dto.Lineas.Any())
				return BadRequest(new { error = "Datos del pedido inválidos" });


			var cliente = await _contexto.Clientes.FindAsync(dto.ClienteId);
			if (cliente == null)
				return BadRequest(new { error = $"Cliente {dto.ClienteId} no encontrado" });

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
						 ?? User.Identity?.Name
						 ?? "unknown";

			using var transaction = await _contexto.Database.BeginTransactionAsync();

			try
			{
				decimal subtotal = 0m;
				decimal impuestos = 0m;
				decimal total = 0m;

				var pedido = new PedidoAD
				{
					ClienteId = dto.ClienteId,
					UsuarioId = userId,
					Fecha = DateTime.Now,
					Estado = "Pendiente",
					Subtotal = 0m,
					Impuestos = 0m,
					Total = 0m,
					Detalles = new List<PedidoDetalleAD>()
				};

				foreach (var linea in dto.Lineas)
				{
					var producto = await _contexto.Productos.FirstOrDefaultAsync(p => p.Id == linea.ProductoId && p.Activo);
					if (producto == null)
						return BadRequest(new { error = $"Producto {linea.ProductoId} no encontrado o inactivo" });

					if (linea.Cantidad <= 0)
						return BadRequest(new { error = "La cantidad debe ser mayor a 0" });

					if (producto.Stock < linea.Cantidad)
						return BadRequest(new { error = $"Stock insuficiente para el producto {producto.Nombre} (disponible: {producto.Stock})" });

					var precioUnit = producto.Precio;
					var importe = precioUnit * linea.Cantidad;

					decimal descuentoAmount = 0m;
					var tipo = (linea.DescuentoTipo ?? "porc").ToLowerInvariant();
					if (tipo == "porc")
						descuentoAmount = importe * (linea.Descuento / 100m);
					else
						descuentoAmount = linea.Descuento;

					if (descuentoAmount < 0) descuentoAmount = 0;
					if (descuentoAmount > importe) descuentoAmount = importe;

					var baseGravable = importe - descuentoAmount;
					var impuestoLinea = baseGravable * (producto.ImpuestoPorc / 100m);
					var totalLinea = baseGravable + impuestoLinea;

					var detalle = new PedidoDetalleAD
					{
						ProductoId = producto.Id,
						Cantidad = linea.Cantidad,
						PrecioUnit = precioUnit,
						Descuento = descuentoAmount,
						ImpuestoPorc = producto.ImpuestoPorc,
						TotalLinea = Math.Round(totalLinea, 2)
					};

					pedido.Detalles.Add(detalle);

					
					producto.Stock -= linea.Cantidad;
					_contexto.Productos.Update(producto);

					subtotal += baseGravable;
					impuestos += impuestoLinea;
					total += totalLinea;
				}

				pedido.Subtotal = Math.Round(subtotal, 2);
				pedido.Impuestos = Math.Round(impuestos, 2);
				pedido.Total = Math.Round(total, 2);

				_contexto.Pedidos.Add(pedido);
				await _contexto.SaveChangesAsync();

				await transaction.CommitAsync();

				var resultado = new ResultadoCrearPedidoDto
				{
					PedidoId = pedido.Id,
					Subtotal = pedido.Subtotal,
					Impuestos = pedido.Impuestos,
					Total = pedido.Total
				};

				return Ok(resultado);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return StatusCode(500, new { error = "Error al crear el pedido", detail = ex.Message });
			}
			*/
		}
	}
}