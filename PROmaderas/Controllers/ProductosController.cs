using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using Microsoft.AspNetCore.Authorization;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
	[Authorize(Roles = Roles.Administrador + "," + Roles.Gerente + "," + Roles.OperadorDePlanta)]
	public class ProductosController : Controller
	{
		private readonly IProductoLogica _productoLogica;
		private readonly IWebHostEnvironment _env;

		public ProductosController(IProductoLogica productoLogica, IWebHostEnvironment env)
		{
			_productoLogica = productoLogica;
			_env = env;
		}

		private async Task<List<CategoriaAD>> ObtenerCategoriasParaEdicion(int categoriaIdActual)
		{
			var categorias = await _productoLogica.ObtenerCategorias();
			if (categorias.Any(c => c.Id == categoriaIdActual))
			{
				return categorias;
			}

			var categoriaActual = await _productoLogica.ObtenerCategoriaPorId(categoriaIdActual);
			if (categoriaActual != null)
			{
				categorias.Add(categoriaActual);
				categorias = categorias.OrderBy(c => c.Nombre).ToList();
			}

			return categorias;
		}

		
		public async Task<IActionResult> Index(string? filtroNombre, int? categoriaId, int pagina = 1)
		{
			int registrosPorPagina = 10;

			var (productos, totalRegistros) = await _productoLogica.ObtenerPaginado(
				pagina,
				registrosPorPagina,
				filtroNombre,
				categoriaId);

			var categorias = await _productoLogica.ObtenerCategorias();

			ViewBag.Categorias = categorias;
			ViewBag.FiltroNombre = filtroNombre;
			ViewBag.CategoriaId = categoriaId;
			ViewBag.PaginaActual = pagina;
			ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

			return View(productos);
		}

		
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return NotFound();

			var producto = await _productoLogica.ObtenerPorId(id.Value);
			if (producto == null)
				return NotFound();

			return View(producto);
		}

		// Sprint 0 PROMADERAS: la tabla TipoTarima exige Codigo (UNIQUE), Medida y
		// otros campos NOT NULL que el modelo ProductoAD no tiene. Stock se
		// gestiona en una tabla aparte (InventarioMovimiento). Por eso
		// Create/Edit/Delete/AjustarStock quedan en construcción y se reemplazarán
		// cuando se conecte el flujo completo. Index/Details siguen funcionando.

		private IActionResult ProductoEnConstruccion()
		{
			ViewBag.Modulo = "La creación, edición, eliminación y ajuste de stock de productos";
			ViewBag.Detalle = "PROMADERAS gestiona productos en la tabla TipoTarima (Codigo, Medida, etc.) y el stock vía InventarioMovimiento. Estará disponible en el próximo sprint.";
			return View("EnConstruccion");
		}

		public IActionResult Create() => ProductoEnConstruccion();

		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Create(ProductoAD producto, IFormFile? imagen) => ProductoEnConstruccion();

		public IActionResult Edit(int? id) => ProductoEnConstruccion();

		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Edit(int id, ProductoAD producto, IFormFile? imagen) => ProductoEnConstruccion();

		public IActionResult Delete(int? id) => ProductoEnConstruccion();

		[HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id) => ProductoEnConstruccion();

		[Authorize(Roles = Roles.Administrador + "," + Roles.OperadorDePlanta)]
		public IActionResult AjustarStock(int? id) => ProductoEnConstruccion();

		[HttpPost, ValidateAntiForgeryToken]
		[Authorize(Roles = Roles.Administrador + "," + Roles.OperadorDePlanta)]
		public IActionResult AjustarStock(int id, int cantidad) => ProductoEnConstruccion();
	}
}