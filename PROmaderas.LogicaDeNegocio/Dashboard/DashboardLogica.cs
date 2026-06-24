using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Dashboard
{
	public class DashboardLogica : IDashboardLogica
	{
		private readonly IDashboardRepositorio _repositorio;

		public DashboardLogica(IDashboardRepositorio repositorio)
		{
			_repositorio = repositorio;
		}

		public async Task<DashboardFinancieroDTO> ObtenerDashboardFinancieroAsync()
		{
			var dashboard = await _repositorio.ObtenerDashboardFinancieroAsync();

			dashboard.FechaUltimaActualizacion = DateTime.Now;

			return dashboard;
		}
	}
}