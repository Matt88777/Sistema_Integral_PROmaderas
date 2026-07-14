using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
	public interface IPolizaINSRepositorio
	{
		Task<List<PolizaINSAD>> ObtenerTodas();

		Task<PolizaINSAD?> ObtenerPorId(int idPolizaINS);

		Task<List<PolizaINSAD>> ObtenerPorEmpleado(int idEmpleado);

		Task<PolizaINSAD?> ObtenerPolizaVigente(
			int idEmpleado,
			DateTime fechaEvaluacion);

		Task<List<PolizaINSAD>> ObtenerProximasAVencer(
			DateTime fechaDesde,
			DateTime fechaHasta);

		Task<bool> ExisteNumeroPoliza(
			string numeroPoliza,
			int? idPolizaExcluir = null);

		Task Crear(PolizaINSAD poliza);

		Task Actualizar(PolizaINSAD poliza);

		Task DesactivarPolizasDelEmpleado(
			int idEmpleado,
			int? idPolizaExcluir = null);
	}
}