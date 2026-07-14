using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
	public interface IPolizaINSLogica
	{
		Task<List<PolizaINSAD>> ObtenerTodas();

		Task<PolizaINSAD?> ObtenerPorId(int idPolizaINS);

		Task<List<PolizaINSAD>> ObtenerHistorialEmpleado(
			int idEmpleado);

		Task Registrar(PolizaINSAD poliza);

		Task Desactivar(int idPolizaINS);

		Task<PolizaINSAD?> ObtenerPolizaVigente(
			int idEmpleado,
			DateTime fechaEvaluacion);

		Task<bool> TieneCoberturaVigente(
			int idEmpleado,
			DateTime fechaEvaluacion);

		Task<List<PolizaINSAD>> ObtenerProximasAVencer(
			int diasAnticipacion = 30);
	}
}