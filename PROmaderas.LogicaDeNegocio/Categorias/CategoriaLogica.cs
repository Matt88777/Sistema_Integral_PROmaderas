using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Categorias
{
	public class CategoriaLogica : ICategoriaLogica
	{
		private readonly ICategoriaRepositorio _repositorio;

		public CategoriaLogica(ICategoriaRepositorio repositorio)
		{
			_repositorio = repositorio;
		}

		public async Task<List<CategoriaAD>> ObtenerTodas()
		{
			return await _repositorio.ObtenerTodas();
		}

		public async Task<CategoriaAD?> ObtenerPorId(int id)
		{
			return await _repositorio.ObtenerPorId(id);
		}

		public async Task<CategoriaAD> Crear(CategoriaAD categoria)
		{
			if (string.IsNullOrWhiteSpace(categoria.Nombre))
				throw new ArgumentException("El nombre de la categoría es requerido");

			return await _repositorio.Crear(categoria);
		}

		public async Task<CategoriaAD> Actualizar(CategoriaAD categoria)
		{
			if (string.IsNullOrWhiteSpace(categoria.Nombre))
				throw new ArgumentException("El nombre de la categoría es requerido");

			if (!await _repositorio.Existe(categoria.Id))
				throw new ArgumentException("La categoría no existe");

			return await _repositorio.Actualizar(categoria);
		}

		public async Task<bool> Eliminar(int id)
		{
			return await _repositorio.Eliminar(id);
		}

		public async Task<bool> Existe(int id)
		{
			return await _repositorio.Existe(id);
		}
	}
}