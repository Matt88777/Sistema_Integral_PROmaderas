using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Clientes
{
	public class ClienteRepositorio : IClienteRepositorio
	{
		private readonly Contexto _contexto;

		public ClienteRepositorio(Contexto contexto)
		{
			_contexto = contexto;
		}

		public async Task<List<ClienteAD>> ObtenerTodos()
		{
			return await _contexto.Clientes.ToListAsync();
		}

		public async Task<ClienteAD?> ObtenerPorId(int id)
		{
			return await _contexto.Clientes.FindAsync(id);
		}

		public async Task<ClienteAD> Crear(ClienteAD cliente)
		{
			_contexto.Clientes.Add(cliente);
			await _contexto.SaveChangesAsync();
			return cliente;
		}

		public async Task<ClienteAD> Actualizar(ClienteAD cliente)
		{
			_contexto.Clientes.Update(cliente);
			await _contexto.SaveChangesAsync();
			return cliente;
		}

		public async Task<bool> Eliminar(int id)
		{
			var cliente = await _contexto.Clientes.FindAsync(id);
			if (cliente == null)
				return false;

			bool tienePedidos = await _contexto.Pedidos.AnyAsync(p => p.ClienteId == id);
			if (tienePedidos)
				throw new InvalidOperationException(
					"No se puede eliminar el cliente porque tiene pedidos asociados.");

			_contexto.Clientes.Remove(cliente);
			await _contexto.SaveChangesAsync();
			return true;
		}

		public async Task<bool> Existe(int id)
		{
			return await _contexto.Clientes.AnyAsync(c => c.Id == id);
		}

		public async Task<List<ClienteAD>> BuscarPorNombre(string nombre)
		{
			return await _contexto.Clientes
				.Where(c => c.Nombre.Contains(nombre))
				.ToListAsync();
		}

		public async Task<(List<ClienteAD> clientes, int totalRegistros)> ObtenerPaginado(
			int pagina,
			int registrosPorPagina,
			string? filtroNombre)
		{
			var query = _contexto.Clientes.AsQueryable();

			if (!string.IsNullOrWhiteSpace(filtroNombre))
			{
				query = query.Where(c => c.Nombre.Contains(filtroNombre));
			}

			var totalRegistros = await query.CountAsync();

			var clientes = await query
				.Skip((pagina - 1) * registrosPorPagina)
				.Take(registrosPorPagina)
				.ToListAsync();

			return (clientes, totalRegistros);
		}
	}
}