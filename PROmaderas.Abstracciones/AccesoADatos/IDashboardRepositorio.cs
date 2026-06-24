using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
	public interface IDashboardRepositorio
	{
		Task<DashboardFinancieroDTO> ObtenerDashboardFinancieroAsync();
	}
}