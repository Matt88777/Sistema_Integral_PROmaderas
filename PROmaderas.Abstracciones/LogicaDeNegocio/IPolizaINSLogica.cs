using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
	public interface IPolizaINSLogica
	{
		Task<List<PolizaINSAD>> ObtenerTodas();

		Task<PolizaINSAD?> ObtenerPorId(
			int idPoliza);

		Task<List<PolizaINSAD>>
			ObtenerHistorialEmpleado(
				int idEmpleado);

		Task Registrar(
			PolizaINSAD poliza,
			List<int> idsEmpleados);

		Task Desactivar(
			int idPoliza);

		Task AsignarEmpleado(
			int idPoliza,
			int idEmpleado,
			DateTime fechaAsignacion);

		Task ExcluirEmpleado(
			int idPoliza,
			int idEmpleado,
			DateTime fechaExclusion);

		Task<PolizaINSAD?> ObtenerPolizaVigente(
			int idEmpleado,
			DateTime fechaEvaluacion);

		Task<bool> TieneCoberturaVigente(
			int idEmpleado,
			DateTime fechaEvaluacion);

		Task<List<PolizaINSAD>>
			ObtenerProximasAVencer(
				int diasAnticipacion = 30);
	}
}