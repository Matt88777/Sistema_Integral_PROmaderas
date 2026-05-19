using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Categorias
{
	// Sprint 0 PROMADERAS: la BD nueva no tiene tabla Categoria. Este repositorio
	// devuelve una lista dummy en memoria con una sola categoría ("Tarimas") para
	// que las vistas de productos sigan funcionando. Las operaciones de escritura
	// quedan deshabilitadas (no-op).
	public class CategoriaRepositorio : ICategoriaRepositorio
	{
		private static readonly List<CategoriaAD> CategoriasDummy = new()
		{
			new CategoriaAD { Id = 1, Nombre = "Tarimas", Activo = true }
		};

		public CategoriaRepositorio(Contexto contexto)
		{
			// El contexto se inyecta para mantener la cadena de DI, pero no se usa.
			_ = contexto;
		}

		public Task<List<CategoriaAD>> ObtenerTodas()
		{
			return Task.FromResult(CategoriasDummy.ToList());
		}

		public Task<CategoriaAD?> ObtenerPorId(int id)
		{
			return Task.FromResult(CategoriasDummy.FirstOrDefault(c => c.Id == id));
		}

		public Task<CategoriaAD> Crear(CategoriaAD categoria)
		{
			throw new InvalidOperationException(
				"Crear categorías está deshabilitado en Sprint 0 (la BD nueva no usa el concepto de Categoria).");
		}

		public Task<CategoriaAD> Actualizar(CategoriaAD categoria)
		{
			throw new InvalidOperationException(
				"Actualizar categorías está deshabilitado en Sprint 0.");
		}

		public Task<bool> Eliminar(int id)
		{
			throw new InvalidOperationException(
				"Eliminar categorías está deshabilitado en Sprint 0.");
		}

		public Task<bool> Existe(int id)
		{
			return Task.FromResult(CategoriasDummy.Any(c => c.Id == id));
		}
	}
}
