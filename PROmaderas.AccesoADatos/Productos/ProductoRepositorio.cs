using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Productos
{
	// Sprint 0 PROMADERAS: ProductoAD se mapea a la tabla TipoTarima de la BD
	// nueva. Las propiedades CategoriaId, Stock, ImpuestoPorc, ImagenUrl y la
	// navegación Categoria son [Ignore] en EF Core, así que no se usan
	// .Include(p => p.Categoria) ni filtros por CategoriaId aquí.
	public class ProductoRepositorio : IProductoRepositorio
	{
		private readonly Contexto _contexto;
		private readonly ICategoriaRepositorio _categoriaRepositorio;

		public ProductoRepositorio(Contexto contexto, ICategoriaRepositorio categoriaRepositorio)
		{
			_contexto = contexto;
			_categoriaRepositorio = categoriaRepositorio;
		}

		public async Task<List<ProductoAD>> ObtenerTodos()
		{
			var productos = await _contexto.Productos
				.Where(p => p.Activo)
				.ToListAsync();

			await AplicarStockActualAsync(productos);

			return productos;
		}

		public async Task<ProductoAD?> ObtenerPorId(int id)
		{
			var producto = await _contexto.Productos
				.FirstOrDefaultAsync(p => p.Id == id);

			if (producto != null)
			{
				producto.Stock = await CalcularStockActualAsync(producto.Id);
			}

			return producto;
		}

		public Task<CategoriaAD?> ObtenerCategoriaPorId(int id)
		{
			return _categoriaRepositorio.ObtenerPorId(id);
		}

		public Task<List<CategoriaAD>> ObtenerCategorias()
		{
			return _categoriaRepositorio.ObtenerTodas();
		}

		public async Task<ProductoAD> Crear(ProductoAD producto)
		{
			_contexto.Productos.Add(producto);
			await _contexto.SaveChangesAsync();
			return producto;
		}

		public async Task<ProductoAD> Actualizar(ProductoAD producto)
		{
			_contexto.Productos.Update(producto);
			await _contexto.SaveChangesAsync();
			return producto;


		}

		public async Task<bool> ExisteDuplicado(string codigo, string nombre)
		{
			codigo = codigo.Trim().ToLower();
			nombre = nombre.Trim().ToLower();

			return await _contexto.Productos
				.AnyAsync(p =>
					p.Codigo.ToLower() == codigo ||
					p.Nombre.ToLower() == nombre);
		}


		public async Task<bool> Eliminar(int id)
		{
			var producto = await ObtenerPorId(id);
			if (producto == null) return false;

			producto.Activo = false;
			await Actualizar(producto);
			return true;
		}

		public async Task<bool> Existe(int id)
		{
			return await _contexto.Productos.AnyAsync(p => p.Id == id);
		}

		public async Task<List<ProductoAD>> BuscarPorNombre(string nombre)
		{
			var productos = await _contexto.Productos
				.Where(p => p.Activo && p.Nombre.Contains(nombre))
				.ToListAsync();

			await AplicarStockActualAsync(productos);

			return productos;
		}

		public async Task<List<ProductoAD>> FiltrarPorCategoria(int? categoriaId)
		{
			_ = categoriaId;

			var productos = await _contexto.Productos
				.Where(p => p.Activo)
				.ToListAsync();

			await AplicarStockActualAsync(productos);

			return productos;
		}

		private async Task<int> CalcularStockActualAsync(int idTipoTarima)
		{
			var entradas = await _contexto.InventarioMovimientos
				.Where(m => m.IdTipoTarima == idTipoTarima &&
							(m.TipoMovimiento == "Entrada" || m.TipoMovimiento == "AjusteEntrada"))
				.SumAsync(m => (int?)m.Cantidad) ?? 0;

			var salidas = await _contexto.InventarioMovimientos
				.Where(m => m.IdTipoTarima == idTipoTarima &&
							(m.TipoMovimiento == "Salida" || m.TipoMovimiento == "AjusteSalida"))
				.SumAsync(m => (int?)m.Cantidad) ?? 0;

			return entradas - salidas;
		}

		private async Task AplicarStockActualAsync(List<ProductoAD> productos)
		{
			foreach (var producto in productos)
			{
				producto.Stock = await CalcularStockActualAsync(producto.Id);
			}
		}

		public async Task<(List<ProductoAD> productos, int totalRegistros)> ObtenerPaginado(
			int pagina,
			int registrosPorPagina,
			string? filtroNombre,
			int? categoriaId)
		{
			// Sprint 0: categoriaId se ignora porque la BD no tiene Categoria.
			_ = categoriaId;

			var query = _contexto.Productos
				.Where(p => p.Activo);

			if (!string.IsNullOrEmpty(filtroNombre))
			{
				query = query.Where(p => p.Nombre.Contains(filtroNombre));
			}

			var totalRegistros = await query.CountAsync();

			var productos = await query
				.OrderBy(p => p.Nombre)
				.Skip((pagina - 1) * registrosPorPagina)
				.Take(registrosPorPagina)
				.ToListAsync();
			await AplicarStockActualAsync(productos);

			return (productos, totalRegistros);
		}



	}
}
