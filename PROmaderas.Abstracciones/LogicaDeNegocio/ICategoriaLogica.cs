using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
	public interface ICategoriaLogica
	{
		Task<List<CategoriaAD>> ObtenerTodas();
		Task<CategoriaAD?> ObtenerPorId(int id);
		Task<CategoriaAD> Crear(CategoriaAD categoria);
		Task<CategoriaAD> Actualizar(CategoriaAD categoria);
		Task<bool> Eliminar(int id);
		Task<bool> Existe(int id);
	}
}