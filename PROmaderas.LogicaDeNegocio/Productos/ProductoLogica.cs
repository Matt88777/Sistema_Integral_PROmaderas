using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Productos
{
	public class ProductoLogica : IProductoLogica
	{
		private readonly IProductoRepositorio _repositorio;

		public ProductoLogica(IProductoRepositorio repositorio)
		{
			_repositorio = repositorio;
		}

		public async Task<List<ProductoAD>> ObtenerTodos()
		{
			return await _repositorio.ObtenerTodos();
		}

		public async Task<ProductoAD?> ObtenerPorId(int id)
		{
			return await _repositorio.ObtenerPorId(id);
		}

		public async Task<CategoriaAD?> ObtenerCategoriaPorId(int id)
		{
			return await _repositorio.ObtenerCategoriaPorId(id);
		}

		public async Task<List<CategoriaAD>> ObtenerCategorias()
		{
			return await _repositorio.ObtenerCategorias();
		}

		public async Task<ProductoAD> Crear(ProductoAD producto)
		{
			if (string.IsNullOrWhiteSpace(producto.Nombre))
				throw new ArgumentException("El nombre del producto es requerido");

			if (producto.Precio <= 0)
				throw new ArgumentException("El precio debe ser mayor a 0");

			if (producto.Stock < 0)
				throw new ArgumentException("El stock no puede ser negativo");

			if (string.IsNullOrWhiteSpace(producto.ImagenUrl))
				throw new ArgumentException("La imagen es obligatoria");

			return await _repositorio.Crear(producto);
		}

		public async Task<ProductoAD> Actualizar(ProductoAD producto)
		{
			if (string.IsNullOrWhiteSpace(producto.Nombre))
				throw new ArgumentException("El nombre del producto es requerido");

			if (producto.Precio <= 0)
				throw new ArgumentException("El precio debe ser mayor a 0");

			if (producto.Stock < 0)
				throw new ArgumentException("El stock no puede ser negativo");

			if (!await _repositorio.Existe(producto.Id))
				throw new ArgumentException("El producto no existe");

			return await _repositorio.Actualizar(producto);
		}

		public async Task<bool> Eliminar(int id)
		{
			return await _repositorio.Eliminar(id);
		}

		public async Task<bool> Existe(int id)
		{
			return await _repositorio.Existe(id);
		}

		public async Task<List<ProductoAD>> BuscarPorNombre(string nombre)
		{
			return await _repositorio.BuscarPorNombre(nombre);
		}

		public async Task<List<ProductoAD>> FiltrarPorCategoria(int? categoriaId)
		{
			return await _repositorio.FiltrarPorCategoria(categoriaId);
		}

		public async Task<(List<ProductoAD> productos, int totalRegistros)> ObtenerPaginado(
			int pagina,
			int registrosPorPagina,
			string? filtroNombre,
			int? categoriaId)
		{
			if (pagina < 1) pagina = 1;
			if (registrosPorPagina < 1) registrosPorPagina = 10;

			return await _repositorio.ObtenerPaginado(pagina, registrosPorPagina, filtroNombre, categoriaId);
		}
		public async Task<ProductoAD> AjustarStock(int id, int cantidad)
		{
			var producto = await _repositorio.ObtenerPorId(id);

			if (producto == null)
				throw new ArgumentException("El producto no existe");

			int nuevoStock = producto.Stock + cantidad;

			if (nuevoStock < 0)
				throw new ArgumentException("El stock no puede quedar negativo");

			producto.Stock = nuevoStock;

			return await _repositorio.Actualizar(producto);
		}
	}
}