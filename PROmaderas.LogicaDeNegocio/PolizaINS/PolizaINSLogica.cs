using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.PolizasINS
{
	public class PolizaINSLogica : IPolizaINSLogica
	{
		private readonly IPolizaINSRepositorio _repositorio;

		public PolizaINSLogica(
			IPolizaINSRepositorio repositorio)
		{
			_repositorio = repositorio;
		}

		public async Task<List<PolizaINSAD>> ObtenerTodas()
		{
			return await _repositorio.ObtenerTodas();
		}

		public async Task<PolizaINSAD?> ObtenerPorId(
			int idPolizaINS)
		{
			if (idPolizaINS <= 0)
				throw new ArgumentException(
					"El identificador de la póliza no es válido.");

			return await _repositorio.ObtenerPorId(idPolizaINS);
		}

		public async Task<List<PolizaINSAD>> ObtenerHistorialEmpleado(
			int idEmpleado)
		{
			if (idEmpleado <= 0)
				throw new ArgumentException(
					"El empleado seleccionado no es válido.");

			return await _repositorio.ObtenerPorEmpleado(idEmpleado);
		}

		public async Task Registrar(PolizaINSAD poliza)
		{
			if (poliza == null)
				throw new ArgumentNullException(
					nameof(poliza),
					"Debe proporcionar la información de la póliza.");

			if (poliza.IdEmpleado <= 0)
				throw new ArgumentException(
					"Debe seleccionar un empleado.");

			poliza.NumeroPoliza =
				poliza.NumeroPoliza?.Trim() ?? string.Empty;

			poliza.Cobertura =
				poliza.Cobertura?.Trim() ?? string.Empty;

			poliza.Observacion =
				string.IsNullOrWhiteSpace(poliza.Observacion)
					? null
					: poliza.Observacion.Trim();

			if (string.IsNullOrWhiteSpace(poliza.NumeroPoliza))
				throw new ArgumentException(
					"El número de póliza es obligatorio.");

			if (poliza.NumeroPoliza.Length > 100)
				throw new ArgumentException(
					"El número de póliza no puede superar los 100 caracteres.");

			if (string.IsNullOrWhiteSpace(poliza.Cobertura))
				throw new ArgumentException(
					"La cobertura de la póliza es obligatoria.");

			if (poliza.Cobertura.Length > 250)
				throw new ArgumentException(
					"La cobertura no puede superar los 250 caracteres.");

			if (poliza.FechaInicio == default)
				throw new ArgumentException(
					"La fecha de inicio es obligatoria.");

			if (poliza.FechaVencimiento == default)
				throw new ArgumentException(
					"La fecha de vencimiento es obligatoria.");

			poliza.FechaInicio = poliza.FechaInicio.Date;
			poliza.FechaVencimiento =
				poliza.FechaVencimiento.Date;

			if (poliza.FechaVencimiento < poliza.FechaInicio)
				throw new ArgumentException(
					"La fecha de vencimiento no puede ser anterior a la fecha de inicio.");

			bool numeroDuplicado =
				await _repositorio.ExisteNumeroPoliza(
					poliza.NumeroPoliza);

			if (numeroDuplicado)
				throw new InvalidOperationException(
					$"Ya existe una póliza registrada con el número '{poliza.NumeroPoliza}'.");

			poliza.Activa = true;
			poliza.FechaRegistro = DateTime.Now;

			await _repositorio.Crear(poliza);
		}

		public async Task Desactivar(int idPolizaINS)
		{
			if (idPolizaINS <= 0)
				throw new ArgumentException(
					"El identificador de la póliza no es válido.");

			PolizaINSAD? poliza =
				await _repositorio.ObtenerPorId(idPolizaINS);

			if (poliza == null)
				throw new InvalidOperationException(
					"No se encontró la póliza seleccionada.");

			if (!poliza.Activa)
				throw new InvalidOperationException(
					"La póliza ya se encuentra inactiva.");

			poliza.Activa = false;

			await _repositorio.Actualizar(poliza);
		}

		public async Task<PolizaINSAD?> ObtenerPolizaVigente(
			int idEmpleado,
			DateTime fechaEvaluacion)
		{
			if (idEmpleado <= 0)
				throw new ArgumentException(
					"El empleado seleccionado no es válido.");

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
			PolizaINSAD? poliza =
				await ObtenerPolizaVigente(
					idEmpleado,
					fechaEvaluacion);

			return poliza != null;
		}

		public async Task<List<PolizaINSAD>> ObtenerProximasAVencer(
			int diasAnticipacion = 30)
		{
			if (diasAnticipacion < 0)
				throw new ArgumentException(
					"Los días de anticipación no pueden ser negativos.");

			DateTime fechaDesde = DateTime.Today;
			DateTime fechaHasta =
				fechaDesde.AddDays(diasAnticipacion);

			return await _repositorio.ObtenerProximasAVencer(
				fechaDesde,
				fechaHasta);
		}
	}
}