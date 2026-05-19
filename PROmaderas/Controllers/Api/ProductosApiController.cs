using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.LogicaDeNegocio;

namespace PROmaderas.UI.Controllers.Api
{
	[ApiController]
	[Route("api/productos")]
	//[Authorize]
	public class ProductosApiController : ControllerBase
	{
		private readonly IProductoLogica _productoLogica;

		public ProductosApiController(IProductoLogica productoLogica)
		{
			_productoLogica = productoLogica;
		}

        
        [HttpGet("buscar")]
        [AllowAnonymous]
		public async Task<IActionResult> Buscar([FromQuery] string q)
		{
			if (string.IsNullOrWhiteSpace(q))
				return BadRequest(new { error = "El parámetro q es requerido" });

			var resultados = await _productoLogica.BuscarPorNombre(q);

			var top = resultados
				.OrderBy(p => p.Nombre)
				.Take(10)
				.Select(p => new
				{
					id = p.Id,
					nombre = p.Nombre,
					precio = p.Precio,
					impuesto = p.ImpuestoPorc,
					stock = p.Stock
				})
				.ToList();

			return Ok(top);
		}
	}
}