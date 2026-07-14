using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
	public interface IPolizaINSRepositorio
	{
		Task<List<PolizaINSAD>> ObtenerTodas();

		Task<PolizaINSAD?> ObtenerPorId(
			int idPoliza);

		Task<List<PolizaINSAD>> ObtenerPorEmpleado(
			int idEmpleado);

		Task<PolizaINSAD?> ObtenerPolizaVigente(
			int idEmpleado,
			DateTime fechaEvaluacion);

		Task<List<PolizaINSAD>> ObtenerProximasAVencer(
			DateTime fechaDesde,
			DateTime fechaHasta);

		Task<bool> ExisteNumeroPoliza(
			string numeroPoliza,
			int? idPolizaExcluir = null);

		Task Crear(
			PolizaINSAD poliza,
			List<int> idsEmpleados);

		Task Actualizar(
			PolizaINSAD poliza);

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
	}
}