using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
	public interface IProductoRepositorio
	{
		Task<List<ProductoAD>> ObtenerTodos();
		Task<ProductoAD?> ObtenerPorId(int id);
		Task<CategoriaAD?> ObtenerCategoriaPorId(int id);
		Task<List<CategoriaAD>> ObtenerCategorias();
		Task<ProductoAD> Crear(ProductoAD producto);
		Task<ProductoAD> Actualizar(ProductoAD producto);
		Task<bool> Eliminar(int id);
		Task<bool> Existe(int id);
		Task<List<ProductoAD>> BuscarPorNombre(string nombre);
		Task<List<ProductoAD>> FiltrarPorCategoria(int? categoriaId);
		Task<(List<ProductoAD> productos, int totalRegistros)> ObtenerPaginado(
			int pagina,
			int registrosPorPagina,
			string? filtroNombre,
			int? categoriaId);
	}
}