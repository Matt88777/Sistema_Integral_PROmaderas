using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
	public interface IIncapacidadRepositorio
	{
		Task<List<IncapacidadAD>> ObtenerTodas();

		Task<IncapacidadAD?> ObtenerPorId(
			int idIncapacidad);

		Task<List<IncapacidadAD>> ObtenerPorEmpleado(
			int idEmpleado);

		Task<List<IncapacidadAD>> ObtenerPorPeriodo(
			int idEmpleado,
			DateTime fechaInicio,
			DateTime fechaFin);

		Task<bool> ExisteTraslape(
			int idEmpleado,
			DateTime fechaInicio,
			DateTime fechaFin,
			int? idIncapacidadExcluir = null);

		Task<bool> ExisteNumeroCertificado(
			string numeroCertificado,
			int? idIncapacidadExcluir = null);

		Task Crear(IncapacidadAD incapacidad);

		Task Actualizar(IncapacidadAD incapacidad);
	}
}