using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Clientes
{
	public class EmpleadosLogica : IClienteLogica
	{
		private readonly IClienteRepositorio _repositorio;

		public EmpleadosLogica(IClienteRepositorio repositorio)
		{
			_repositorio = repositorio;
		}

		public async Task<List<ClienteAD>> ObtenerTodos()
		{
			return await _repositorio.ObtenerTodos();
		}

		public async Task<ClienteAD?> ObtenerPorId(int id)
		{
			return await _repositorio.ObtenerPorId(id);
		}

		public async Task<ClienteAD> Crear(ClienteAD cliente)
		{
			if (string.IsNullOrWhiteSpace(cliente.Nombre))
				throw new ArgumentException("El nombre del cliente es requerido");

			if (string.IsNullOrWhiteSpace(cliente.Cedula))
				throw new ArgumentException("La cédula es requerida");

			if (string.IsNullOrWhiteSpace(cliente.Correo))
				throw new ArgumentException("El correo es requerido");

			return await _repositorio.Crear(cliente);
		}

		public async Task<ClienteAD> Actualizar(ClienteAD cliente)
		{
			if (string.IsNullOrWhiteSpace(cliente.Nombre))
				throw new ArgumentException("El nombre del cliente es requerido");

			if (string.IsNullOrWhiteSpace(cliente.Cedula))
				throw new ArgumentException("La cédula es requerida");

			if (string.IsNullOrWhiteSpace(cliente.Correo))
				throw new ArgumentException("El correo es requerido");

			return await _repositorio.Actualizar(cliente);
		}

		public async Task<bool> Eliminar(int id)
		{
			var existe = await _repositorio.Existe(id);
			if (!existe)
				throw new ArgumentException("El cliente no existe");

			return await _repositorio.Eliminar(id);
		}

		public async Task<bool> Existe(int id)
		{
			return await _repositorio.Existe(id);
		}

		public async Task<List<ClienteAD>> BuscarPorNombre(string nombre)
		{
			if (string.IsNullOrWhiteSpace(nombre))
				return await _repositorio.ObtenerTodos();

			return await _repositorio.BuscarPorNombre(nombre);
		}

		public async Task<(List<ClienteAD> clientes, int totalRegistros)> ObtenerPaginado(
			int pagina,
			int registrosPorPagina,
			string? filtroNombre)
		{
			if (pagina < 1) pagina = 1;
			if (registrosPorPagina < 1) registrosPorPagina = 10;

			return await _repositorio.ObtenerPaginado(pagina, registrosPorPagina, filtroNombre);
		}

		//Obtener historial por cliente
        public async Task<(ClienteAD? cliente, List<PedidoAD> pedidos)> ObtenerHistorialPorCliente(int clienteId)
        {
            return await _repositorio.ObtenerHistorialPorCliente(clienteId);
        }
    }
}