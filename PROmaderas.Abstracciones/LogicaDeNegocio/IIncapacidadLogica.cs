using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
	public interface IIncapacidadLogica
	{
		Task<List<IncapacidadAD>> ObtenerTodas();

		Task<IncapacidadAD?> ObtenerPorId(
			int idIncapacidad);

		Task<List<IncapacidadAD>>
			ObtenerHistorialEmpleado(
				int idEmpleado);

		Task<List<IncapacidadAD>>
			ObtenerPorPeriodo(
				int idEmpleado,
				DateTime fechaInicio,
				DateTime fechaFin);

		Task Registrar(
			IncapacidadAD incapacidad);

		Task Anular(
			int idIncapacidad);

		decimal CalcularDiasDentroPeriodo(
			IncapacidadAD incapacidad,
			DateTime inicioPeriodo,
			DateTime finPeriodo);
	}
}