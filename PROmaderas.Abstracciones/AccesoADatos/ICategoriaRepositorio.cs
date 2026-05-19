using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
	public interface ICategoriaRepositorio
	{
		Task<List<CategoriaAD>> ObtenerTodas();
		Task<CategoriaAD?> ObtenerPorId(int id);
		Task<CategoriaAD> Crear(CategoriaAD categoria);
		Task<CategoriaAD> Actualizar(CategoriaAD categoria);
		Task<bool> Eliminar(int id);
		Task<bool> Existe(int id);
	}
}