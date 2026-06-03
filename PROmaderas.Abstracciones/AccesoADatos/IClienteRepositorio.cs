using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
	public interface IClienteRepositorio
	{
		Task<List<ClienteAD>> ObtenerTodos();
		Task<ClienteAD?> ObtenerPorId(int id);
		Task<ClienteAD> Crear(ClienteAD cliente);
		Task<ClienteAD> Actualizar(ClienteAD cliente, ContextoAuditoria auditoria);
		Task<bool> Eliminar(int id);
		Task<bool> Existe(int id);
		Task<List<ClienteAD>> BuscarPorNombre(string nombre);
		Task<(List<ClienteAD> clientes, int totalRegistros)> ObtenerPaginado(
			int pagina,
			int registrosPorPagina,
			string? filtroNombre);

        //Busqueda historial del cliente
        Task<(ClienteAD? cliente, List<PedidoAD> pedidos)> ObtenerHistorialPorCliente(int clienteId);

    }
}