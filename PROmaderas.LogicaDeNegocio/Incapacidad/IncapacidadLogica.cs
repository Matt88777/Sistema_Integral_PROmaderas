using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Incapacidad
{
	public class IncapacidadLogica :
		IIncapacidadLogica
	{
		private readonly IIncapacidadRepositorio _repositorio;

		public IncapacidadLogica(
			IIncapacidadRepositorio repositorio)
		{
			_repositorio = repositorio;
		}

		public Task<List<IncapacidadAD>> ObtenerTodas()
		{
			return _repositorio.ObtenerTodas();
		}

		public async Task<IncapacidadAD?> ObtenerPorId(
			int idIncapacidad)
		{
			ValidarId(idIncapacidad, "incapacidad");

			return await _repositorio.ObtenerPorId(
				idIncapacidad);
		}

		public async Task<List<IncapacidadAD>>
			ObtenerHistorialEmpleado(int idEmpleado)
		{
			ValidarId(idEmpleado, "empleado");

			return await _repositorio
				.ObtenerPorEmpleado(idEmpleado);
		}

		public async Task<List<IncapacidadAD>>
			ObtenerPorPeriodo(
				int idEmpleado,
				DateTime fechaInicio,
				DateTime fechaFin)
		{
			ValidarId(idEmpleado, "empleado");
			ValidarFechas(fechaInicio, fechaFin);

			return await _repositorio.ObtenerPorPeriodo(
				idEmpleado,
				fechaInicio.Date,
				fechaFin.Date);
		}

		public async Task Registrar(
			IncapacidadAD incapacidad)
		{
			if (incapacidad == null)
				throw new ArgumentNullException(
					nameof(incapacidad));

			ValidarId(
				incapacidad.IdEmpleado,
				"empleado");

			incapacidad.TipoIncapacidad =
				incapacidad.TipoIncapacidad?.Trim()
				?? string.Empty;

			incapacidad.NumeroCertificado =
				incapacidad.NumeroCertificado?.Trim()
				?? string.Empty;

			incapacidad.EntidadEmisora =
				incapacidad.EntidadEmisora?.Trim()
				?? string.Empty;

			incapacidad.Observacion =
				string.IsNullOrWhiteSpace(
					incapacidad.Observacion)
					? null
					: incapacidad.Observacion.Trim();

			if (!TiposIncapacidad.EsValido(
				incapacidad.TipoIncapacidad))
			{
				throw new ArgumentException(
					"El tipo de incapacidad no es válido.");
			}

			if (string.IsNullOrWhiteSpace(
				incapacidad.NumeroCertificado))
			{
				throw new ArgumentException(
					"El número de certificado es obligatorio.");
			}

			if (string.IsNullOrWhiteSpace(
				incapacidad.EntidadEmisora))
			{
				throw new ArgumentException(
					"La entidad emisora es obligatoria.");
			}

			ValidarFechas(
				incapacidad.FechaInicio,
				incapacidad.FechaFin);

			incapacidad.FechaInicio =
				incapacidad.FechaInicio.Date;

			incapacidad.FechaFin =
				incapacidad.FechaFin.Date;

			// La base de Sprint 4 exige la columna Dias.
			// Si la pantalla no envía un valor, se calcula
			// incluyendo inicio y fin.
			if (incapacidad.Dias <= 0)
			{
				incapacidad.Dias =
					(incapacidad.FechaFin -
					 incapacidad.FechaInicio).Days + 1;
			}

			bool certificadoDuplicado =
				await _repositorio
					.ExisteNumeroCertificado(
						incapacidad.NumeroCertificado);

			if (certificadoDuplicado)
			{
				throw new InvalidOperationException(
					$"Ya existe una incapacidad con el certificado " +
					$"'{incapacidad.NumeroCertificado}'.");
			}

			bool traslape =
				await _repositorio.ExisteTraslape(
					incapacidad.IdEmpleado,
					incapacidad.FechaInicio,
					incapacidad.FechaFin);

			if (traslape)
			{
				throw new InvalidOperationException(
					"Las fechas se traslapan con otra incapacidad activa del empleado.");
			}

			incapacidad.Activa = true;
			incapacidad.FechaRegistro = DateTime.Now;

			await _repositorio.Crear(incapacidad);
		}

		public async Task Anular(
			int idIncapacidad)
		{
			ValidarId(idIncapacidad, "incapacidad");

			IncapacidadAD? incapacidad =
				await _repositorio.ObtenerPorId(
					idIncapacidad);

			if (incapacidad == null)
				throw new InvalidOperationException(
					"No se encontró la incapacidad.");

			if (!incapacidad.Activa)
				throw new InvalidOperationException(
					"La incapacidad ya está anulada.");

			incapacidad.Activa = false;

			await _repositorio.Actualizar(
				incapacidad);
		}

		public decimal CalcularDiasDentroPeriodo(
			IncapacidadAD incapacidad,
			DateTime inicioPeriodo,
			DateTime finPeriodo)
		{
			if (incapacidad == null)
				throw new ArgumentNullException(
					nameof(incapacidad));

			ValidarFechas(
				inicioPeriodo,
				finPeriodo);

			if (!incapacidad.Activa)
				return 0m;

			DateTime inicioReal =
				incapacidad.FechaInicio.Date >
				inicioPeriodo.Date
					? incapacidad.FechaInicio.Date
					: inicioPeriodo.Date;

			DateTime finReal =
				incapacidad.FechaFin.Date <
				finPeriodo.Date
					? incapacidad.FechaFin.Date
					: finPeriodo.Date;

			if (finReal < inicioReal)
				return 0m;

			int diasNaturalesTotales =
				(incapacidad.FechaFin.Date -
				 incapacidad.FechaInicio.Date).Days + 1;

			int diasNaturalesPeriodo =
				(finReal - inicioReal).Days + 1;

			if (diasNaturalesTotales <= 0)
				return 0m;

			if (diasNaturalesPeriodo ==
				diasNaturalesTotales)
			{
				return incapacidad.Dias;
			}

			decimal proporcion =
				diasNaturalesPeriodo /
				(decimal)diasNaturalesTotales;

			return Math.Round(
				incapacidad.Dias * proporcion,
				2);
		}

		private static void ValidarFechas(
			DateTime fechaInicio,
			DateTime fechaFin)
		{
			if (fechaInicio == default ||
				fechaFin == default)
			{
				throw new ArgumentException(
					"Las fechas son obligatorias.");
			}

			if (fechaFin.Date <
				fechaInicio.Date)
			{
				throw new ArgumentException(
					"La fecha final no puede ser anterior a la inicial.");
			}
		}

		private static void ValidarId(
			int id,
			string nombre)
		{
			if (id <= 0)
			{
				throw new ArgumentException(
					$"El identificador de {nombre} no es válido.");
			}
		}
	}
}