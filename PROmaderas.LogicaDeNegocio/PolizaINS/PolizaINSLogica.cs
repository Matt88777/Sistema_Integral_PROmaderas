using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.PolizaINS
{
	public class PolizaINSLogica :
		IPolizaINSLogica
	{
		private readonly IPolizaINSRepositorio _repositorio;

		public PolizaINSLogica(
			IPolizaINSRepositorio repositorio)
		{
			_repositorio = repositorio;
		}

		public Task<List<PolizaINSAD>> ObtenerTodas()
		{
			return _repositorio.ObtenerTodas();
		}

		public async Task<PolizaINSAD?> ObtenerPorId(
			int idPoliza)
		{
			ValidarId(idPoliza, "póliza");
			return await _repositorio.ObtenerPorId(idPoliza);
		}

		public async Task<List<PolizaINSAD>>
			ObtenerHistorialEmpleado(int idEmpleado)
		{
			ValidarId(idEmpleado, "empleado");

			return await _repositorio
				.ObtenerPorEmpleado(idEmpleado);
		}

		public async Task Registrar(
			PolizaINSAD poliza,
			List<int> idsEmpleados)
		{
			if (poliza == null)
				throw new ArgumentNullException(
					nameof(poliza));

			idsEmpleados ??= new List<int>();

			idsEmpleados = idsEmpleados
				.Where(id => id > 0)
				.Distinct()
				.ToList();

			if (idsEmpleados.Count == 0)
			{
				throw new ArgumentException(
					"Debe seleccionar al menos un empleado cubierto.");
			}

			poliza.NumeroPoliza =
				poliza.NumeroPoliza?.Trim()
				?? string.Empty;

			poliza.TipoPoliza =
				poliza.TipoPoliza?.Trim()
				?? string.Empty;

			poliza.Aseguradora =
				poliza.Aseguradora?.Trim()
				?? string.Empty;

			poliza.Observacion =
				string.IsNullOrWhiteSpace(
					poliza.Observacion)
					? null
					: poliza.Observacion.Trim();

			if (string.IsNullOrWhiteSpace(
				poliza.NumeroPoliza))
			{
				throw new ArgumentException(
					"El número de póliza es obligatorio.");
			}

			if (poliza.NumeroPoliza.Length > 50)
			{
				throw new ArgumentException(
					"El número de póliza no puede superar 50 caracteres.");
			}

			if (string.IsNullOrWhiteSpace(
				poliza.TipoPoliza))
			{
				throw new ArgumentException(
					"El tipo de póliza es obligatorio.");
			}

			if (string.IsNullOrWhiteSpace(
				poliza.Aseguradora))
			{
				throw new ArgumentException(
					"La aseguradora es obligatoria.");
			}

			if (poliza.FechaInicio == default ||
				poliza.FechaVencimiento == default)
			{
				throw new ArgumentException(
					"Las fechas de vigencia son obligatorias.");
			}

			poliza.FechaInicio =
				poliza.FechaInicio.Date;

			poliza.FechaVencimiento =
				poliza.FechaVencimiento.Date;

			if (poliza.FechaVencimiento <
				poliza.FechaInicio)
			{
				throw new ArgumentException(
					"La fecha de vencimiento no puede ser anterior al inicio.");
			}

			if (poliza.MontoAsegurado < 0 ||
				poliza.Prima < 0)
			{
				throw new ArgumentException(
					"Los montos de la póliza no pueden ser negativos.");
			}

			bool duplicada =
				await _repositorio.ExisteNumeroPoliza(
					poliza.NumeroPoliza);

			if (duplicada)
			{
				throw new InvalidOperationException(
					$"Ya existe la póliza '{poliza.NumeroPoliza}'.");
			}

			poliza.Estado = true;
			poliza.FechaCreacion = DateTime.Now;

			await _repositorio.Crear(
				poliza,
				idsEmpleados);
		}

		public async Task Desactivar(
			int idPoliza)
		{
			ValidarId(idPoliza, "póliza");

			PolizaINSAD? poliza =
				await _repositorio.ObtenerPorId(idPoliza);

			if (poliza == null)
			{
				throw new InvalidOperationException(
					"No se encontró la póliza.");
			}

			if (!poliza.Estado)
			{
				throw new InvalidOperationException(
					"La póliza ya está inactiva.");
			}

			await _repositorio.Desactivar(idPoliza);
		}

		public async Task AsignarEmpleado(
			int idPoliza,
			int idEmpleado,
			DateTime fechaAsignacion)
		{
			ValidarId(idPoliza, "póliza");
			ValidarId(idEmpleado, "empleado");

			PolizaINSAD? poliza =
				await _repositorio.ObtenerPorId(idPoliza);

			if (poliza == null)
				throw new InvalidOperationException(
					"No se encontró la póliza.");

			if (!poliza.Estado)
				throw new InvalidOperationException(
					"No se pueden asignar empleados a una póliza inactiva.");

			if (fechaAsignacion.Date <
				poliza.FechaInicio.Date ||
				fechaAsignacion.Date >
				poliza.FechaVencimiento.Date)
			{
				throw new ArgumentException(
					"La fecha de asignación debe estar dentro de la vigencia.");
			}

			await _repositorio.AsignarEmpleado(
				idPoliza,
				idEmpleado,
				fechaAsignacion);
		}

		public async Task ExcluirEmpleado(
			int idPoliza,
			int idEmpleado,
			DateTime fechaExclusion)
		{
			ValidarId(idPoliza, "póliza");
			ValidarId(idEmpleado, "empleado");

			await _repositorio.ExcluirEmpleado(
				idPoliza,
				idEmpleado,
				fechaExclusion);
		}

		public async Task<PolizaINSAD?>
			ObtenerPolizaVigente(
				int idEmpleado,
				DateTime fechaEvaluacion)
		{
			ValidarId(idEmpleado, "empleado");

			if (fechaEvaluacion == default)
				throw new ArgumentException(
					"La fecha de evaluación no es válida.");

			return await _repositorio.ObtenerPolizaVigente(
				idEmpleado,
				fechaEvaluacion.Date);
		}

		public async Task<bool> TieneCoberturaVigente(
			int idEmpleado,
			DateTime fechaEvaluacion)
		{
			return await ObtenerPolizaVigente(
				idEmpleado,
				fechaEvaluacion) != null;
		}

		public async Task<List<PolizaINSAD>>
			ObtenerProximasAVencer(
				int diasAnticipacion = 30)
		{
			if (diasAnticipacion < 0)
				throw new ArgumentException(
					"Los días de anticipación no pueden ser negativos.");

			DateTime desde = DateTime.Today;
			DateTime hasta =
				desde.AddDays(diasAnticipacion);

			return await _repositorio
				.ObtenerProximasAVencer(
					desde,
					hasta);
		}

		private static void ValidarId(
			int id,
			string nombre)
		{
			if (id <= 0)
				throw new ArgumentException(
					$"El identificador de {nombre} no es válido.");
		}
	}
}