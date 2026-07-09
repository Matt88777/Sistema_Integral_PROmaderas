using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
	public interface IProductoLogica
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
    int? categoriaId,
    bool? filtroEstado);
        Task<ProductoAD> AjustarStock(int id, int cantidad);
        Task<List<InventarioExistenciaDTO>> ObtenerExistenciasActuales(int? idTipoTarima);
        Task<List<InventarioMovimientoDTO>> ObtenerHistorialMovimientos(int? idTipoTarima);
        Task RegistrarAjusteInventario(AjusteInventarioDTO ajuste);
        Task<ProductoAD> CambiarEstadoTipoTarima(int id);
    }
}