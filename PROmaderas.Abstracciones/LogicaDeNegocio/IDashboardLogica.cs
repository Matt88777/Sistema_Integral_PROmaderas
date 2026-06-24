using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
	public interface IDashboardLogica
	{
		Task<DashboardFinancieroDTO> ObtenerDashboardFinancieroAsync();
	}
}