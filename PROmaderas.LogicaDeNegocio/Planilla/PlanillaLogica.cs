using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Planilla
{
	public class PlanillaLogica : IPlanillaLogica
	{
		private readonly IPlanillaRepositorio _repo;
		private readonly IParametroPlanillaRepositorio _parametros;
		private readonly IPolizaINSLogica _polizaINSLogica;
		private readonly IIncapacidadLogica _incapacidadLogica;
		private readonly IVacacionLogica _vacacionLogica;

		private static readonly string[] EstadosValidos =
			{ "Borrador", "Revisada", "Aprobada", "Pagada" };

		public PlanillaLogica(
	IPlanillaRepositorio repo,
	IParametroPlanillaRepositorio parametros,
	IPolizaINSLogica polizaINSLogica,
	IIncapacidadLogica incapacidadLogica,
	IVacacionLogica vacacionLogica)
		{
			_repo = repo;
			_parametros = parametros;
			_polizaINSLogica = polizaINSLogica;
			_incapacidadLogica = incapacidadLogica;
			_vacacionLogica = vacacionLogica;
		}

		public async Task<List<PlanillaPeriodoAD>> ObtenerPeriodos()
			=> await _repo.ObtenerPeriodos();



		public async Task<PlanillaPeriodoAD?> ObtenerPeriodoPorId(int id)
			=> await _repo.ObtenerPeriodoPorId(id);

		public async Task<PlanillaPeriodoAD> CrearPeriodo(PlanillaPeriodoAD periodo)
		{
			if (periodo.FechaFin <= periodo.FechaInicio)
				throw new ArgumentException("La fecha de fin debe ser posterior a la de inicio.");

			if (string.IsNullOrWhiteSpace(periodo.TipoPeriodo))
				throw new ArgumentException("El tipo de período es requerido.");

			periodo.Estado = "Borrador";
			return await _repo.CrearPeriodo(periodo);
		}

		public async Task CambiarEstadoPeriodo(int id, string nuevoEstado, ContextoAuditoria auditoria)
		{
			if (!EstadosValidos.Contains(nuevoEstado))
				throw new ArgumentException($"Estado '{nuevoEstado}' no válido.");

			await _repo.CambiarEstadoPeriodo(id, nuevoEstado, auditoria);
		}

		public async Task EliminarPeriodo(int id)
		{
			var periodo = await _repo.ObtenerPeriodoPorId(id)
				?? throw new ArgumentException("Período no encontrado.");

			if (periodo.Estado != "Borrador")
				throw new InvalidOperationException("Solo se pueden eliminar períodos en estado Borrador.");

			await _repo.EliminarPeriodo(id);
		}

		public async Task<List<PlanillaDetalleFinancieroAD>> ObtenerDetallesPorPeriodo(int idPeriodo)
			=> await _repo.ObtenerDetallesPorPeriodo(idPeriodo);

		public async Task RegistrarHoras(PlanillaDetalleFormVM vm, ContextoAuditoria auditoria)
		{
			// PLA-HU-019: los parámetros se resuelven a la FECHA DEL PERÍODO, no a la de hoy.
			// Recalcular un período viejo tiene que dar el mismo número que dio en su momento.
			var periodo = await _repo.ObtenerPeriodoPorId(vm.IdPlanillaPeriodo)
				?? throw new ArgumentException("Período no encontrado.");

			var fecha = periodo.FechaInicio;
			var p = await _parametros.ObtenerVigentes(fecha);   // UN query para los 12 parámetros

			var (salarioBase, montoExtra, brutoSinAjustar) =
	Calcular(
		vm.SalarioMensual,
		vm.HorasExtra,
		p,
		fecha);

			// PLA-HU-014: obtener y calcular las incapacidades
			// que coinciden con este periodo.
			var ajusteIncapacidad =
				await CalcularAjusteIncapacidades(
					vm.IdEmpleado,
					vm.SalarioMensual,
					periodo,
					p);

			// PLA-HU-013: calcular vacaciones disfrutadas
			// dentro del periodo.
			var ajusteVacaciones =
				await CalcularVacaciones(
					vm.IdEmpleado,
					vm.SalarioMensual,
					periodo);

			decimal bruto =
				Math.Max(
					0m,
					Math.Round(
						brutoSinAjustar -
						ajusteIncapacidad.rebajo,
						2));

			var (ccss, renta, totalDed) =
				CalcularDeducciones(
					bruto,
					p,
					fecha);

			// PLA-HU-007: deducciones internas configurables
			var internas = await _repo.ObtenerDeduccionesActivasDeEmpleado(vm.IdEmpleado);
			var totalInternas = CalcularDeduccionesInternas(internas, bruto);
			totalDed += totalInternas;

			// PLA-HU-018: el empleado debe tener cobertura durante
			// todo el período de planilla.
			bool coberturaAlInicio =
				await _polizaINSLogica.TieneCoberturaVigente(
					vm.IdEmpleado,
					periodo.FechaInicio);

			bool coberturaAlFinal =
				await _polizaINSLogica.TieneCoberturaVigente(
					vm.IdEmpleado,
					periodo.FechaFin);

			bool tieneCoberturaVigente =
				coberturaAlInicio && coberturaAlFinal;

			string? advertenciaPoliza =
				tieneCoberturaVigente
					? null
					: $"El empleado no cuenta con una póliza del INS vigente " +
					  $"para todo el período del " +
					  $"{periodo.FechaInicio:dd/MM/yyyy} al " +
					  $"{periodo.FechaFin:dd/MM/yyyy}.";

			var detalle = new PlanillaDetalleFinancieroAD
			{
				IdPlanillaPeriodo = vm.IdPlanillaPeriodo,
				IdEmpleado = vm.IdEmpleado,
				SalarioBase = salarioBase,
				HorasOrdinarias = vm.HorasOrdinarias,
				HorasExtra = vm.HorasExtra,
				MontoHorasExtra = montoExtra,
				SalarioBruto = bruto,
				DeduccionCCSS = ccss,
				DeduccionRenta = renta,
				DeduccionesInternas = totalInternas,
				TotalDeducciones = totalDed,
				SalarioNeto = bruto - totalDed,
				TienePolizaINSVigente = tieneCoberturaVigente,
				AdvertenciaPolizaINS = advertenciaPoliza,

				DiasIncapacidad = ajusteIncapacidad.diasIncapacidad,

				MontoDiasIncapacidad = ajusteIncapacidad.montoDias,

				MontoPagoPatronalIncapacidad = ajusteIncapacidad.pagoPatronal,

				RebajoIncapacidad = ajusteIncapacidad.rebajo,

				DetalleIncapacidad = ajusteIncapacidad.detalle,

				DiasVacaciones = ajusteVacaciones.diasVacaciones,

				MontoVacaciones = ajusteVacaciones.montoVacaciones,

				DetalleVacaciones = ajusteVacaciones.detalle
			};

			await _repo.AgregarDetalle(detalle);
		}

		public async Task ActualizarHoras(int idDetalle, decimal salarioMensual,
			decimal horasOrdinarias, decimal horasExtra, ContextoAuditoria auditoria)
		{
			var detalle = await _repo.ObtenerDetallePorId(idDetalle)
				?? throw new ArgumentException("Detalle no encontrado.");

			// Misma regla que en RegistrarHoras: la fecha sale del período del detalle.
			var periodo = await _repo.ObtenerPeriodoPorId(detalle.IdPlanillaPeriodo)
				?? throw new ArgumentException("Período no encontrado.");

			var fecha = periodo.FechaInicio;
			var p = await _parametros.ObtenerVigentes(fecha);

			var (salarioBase, montoExtra, brutoSinAjustar) =
	Calcular(
		salarioMensual,
		horasExtra,
		p,
		fecha);

			// PLA-HU-014: recalcular las incapacidades
			// cuando se modifica el detalle.
			var ajusteIncapacidad =
				await CalcularAjusteIncapacidades(
					detalle.IdEmpleado,
					salarioMensual,
					periodo,
					p);

			var ajusteVacaciones =
				await CalcularVacaciones(
					detalle.IdEmpleado,
					salarioMensual,
					periodo);

			decimal bruto =
				Math.Max(
					0m,
					Math.Round(
						brutoSinAjustar -
						ajusteIncapacidad.rebajo,
						2));

			var (ccss, renta, totalDed) =
				CalcularDeducciones(
					bruto,
					p,
					fecha);

			var internas = await _repo.ObtenerDeduccionesActivasDeEmpleado(detalle.IdEmpleado);
			var totalInternas = CalcularDeduccionesInternas(internas, bruto);
			totalDed += totalInternas;

			// PLA-HU-018: volver a evaluar la cobertura
			// cuando el detalle de planilla se recalcula.
			bool coberturaAlInicio =
				await _polizaINSLogica.TieneCoberturaVigente(
					detalle.IdEmpleado,
					periodo.FechaInicio);

			bool coberturaAlFinal =
				await _polizaINSLogica.TieneCoberturaVigente(
					detalle.IdEmpleado,
					periodo.FechaFin);

			bool tieneCoberturaVigente =
				coberturaAlInicio && coberturaAlFinal;

			string? advertenciaPoliza =
				tieneCoberturaVigente
					? null
					: $"El empleado no cuenta con una póliza del INS vigente " +
					  $"para todo el período del " +
					  $"{periodo.FechaInicio:dd/MM/yyyy} al " +
					  $"{periodo.FechaFin:dd/MM/yyyy}.";

			detalle.SalarioBase = salarioBase;
			detalle.HorasOrdinarias = horasOrdinarias;
			detalle.HorasExtra = horasExtra;
			detalle.MontoHorasExtra = montoExtra;
			detalle.SalarioBruto = bruto;
			detalle.DeduccionCCSS = ccss;
			detalle.DeduccionRenta = renta;
			detalle.DeduccionesInternas = totalInternas;
			detalle.TotalDeducciones = totalDed;
			detalle.SalarioNeto = bruto - totalDed;

			detalle.TienePolizaINSVigente =
	tieneCoberturaVigente;

			detalle.AdvertenciaPolizaINS =
				advertenciaPoliza;

			detalle.DiasIncapacidad =
			ajusteIncapacidad.diasIncapacidad;

			detalle.MontoDiasIncapacidad =
				ajusteIncapacidad.montoDias;

			detalle.MontoPagoPatronalIncapacidad =
				ajusteIncapacidad.pagoPatronal;

			detalle.RebajoIncapacidad =
				ajusteIncapacidad.rebajo;

			detalle.DetalleIncapacidad =
				ajusteIncapacidad.detalle;

			detalle.DiasVacaciones =
				ajusteVacaciones.diasVacaciones;

			detalle.MontoVacaciones =
				ajusteVacaciones.montoVacaciones;

			detalle.DetalleVacaciones =
				ajusteVacaciones.detalle;

			await _repo.ActualizarDetalle(detalle);
		}

		private async Task<(
	decimal diasVacaciones,
	decimal montoVacaciones,
	string? detalle)>
	CalcularVacaciones(
		int idEmpleado,
		decimal salarioMensual,
		PlanillaPeriodoAD periodo)
		{
			List<VacacionAD> vacaciones =
				await _vacacionLogica.ObtenerPorPeriodo(
					idEmpleado,
					periodo.FechaInicio,
					periodo.FechaFin);

			if (vacaciones.Count == 0)
			{
				return (0m, 0m, null);
			}

			decimal salarioDiario =
				Math.Round(
					salarioMensual / 30m,
					2);

			decimal totalDias = 0m;
			List<string> detalles = new();

			foreach (VacacionAD vacacion in vacaciones)
			{
				decimal diasEnPeriodo =
					_vacacionLogica.CalcularDiasDentroPeriodo(
						vacacion,
						periodo.FechaInicio,
						periodo.FechaFin);

				if (diasEnPeriodo <= 0)
					continue;

				totalDias += diasEnPeriodo;

				detalles.Add(
					$"{diasEnPeriodo:N2} día(s) del " +
					$"{vacacion.FechaInicio:dd/MM/yyyy} al " +
					$"{vacacion.FechaFin:dd/MM/yyyy}");
			}

			decimal montoVacaciones =
				Math.Round(
					salarioDiario * totalDias,
					2);

			return (
				Math.Round(totalDias, 2),
				montoVacaciones,
				detalles.Count == 0
					? null
					: string.Join("; ", detalles));
		}

		public async Task EliminarDetalle(int idDetalle)
			=> await _repo.EliminarDetalle(idDetalle);

		public async Task<List<EmpleadoAD>> ObtenerEmpleadosActivos()
			=> await _repo.ObtenerEmpleadosActivos();

		// ── cálculos ──────────────────────────────────────────────────────────
		// Siguen siendo funciones PURAS: no tocan la BD. Reciben ya resueltos los parámetros
		// vigentes a la fecha del período (y esa fecha, solo para el mensaje de error).

		private static (decimal salarioBase, decimal montoExtra, decimal bruto) Calcular(
			decimal salarioMensual, decimal horasExtra,
			IReadOnlyDictionary<string, decimal> parametros, DateTime fecha)
		{
			// Cantidad: se usa tal cual, como divisor.
			var horasMes = Obtener(parametros, ParametrosPlanilla.HorasMes, fecha);
			if (horasMes <= 0)
				throw new InvalidOperationException(
					$"El parámetro '{ParametrosPlanilla.HorasMes}' debe ser mayor a cero (valor actual: {horasMes}).");

			// FACTOR, no porcentaje: NO se divide entre 100 (vale 1.5, no 150).
			var factorHoraExtra = Obtener(parametros, ParametrosPlanilla.FactorHoraExtra, fecha);

			var valorHora = salarioMensual / horasMes;
			var montoExtra = Math.Round(valorHora * factorHoraExtra * horasExtra, 2);
			var bruto = Math.Round(salarioMensual + montoExtra, 2);
			return (salarioMensual, montoExtra, bruto);
		}

		private static (decimal ccss, decimal renta, decimal total) CalcularDeducciones(
			decimal bruto, IReadOnlyDictionary<string, decimal> parametros, DateTime fecha)
		{
			// PORCENTAJE: la tabla guarda 10.67 (= 10.67%), así que se divide entre 100.
			var porcCCSS = Obtener(parametros, ParametrosPlanilla.PorcentajeCCSS, fecha);
			var ccss = Math.Round(bruto * (porcCCSS / 100m), 2);

			// Tramos de mayor a menor: se le cobra a cada tramo solo su excedente y se baja el
			// techo. Los *Piso son colones (tal cual); los *Porc son porcentajes (entre 100).
			var t4Piso = Obtener(parametros, ParametrosPlanilla.RentaTramo4Piso, fecha);
			var t4Porc = Obtener(parametros, ParametrosPlanilla.RentaTramo4Porc, fecha);
			var t3Piso = Obtener(parametros, ParametrosPlanilla.RentaTramo3Piso, fecha);
			var t3Porc = Obtener(parametros, ParametrosPlanilla.RentaTramo3Porc, fecha);
			var t2Piso = Obtener(parametros, ParametrosPlanilla.RentaTramo2Piso, fecha);
			var t2Porc = Obtener(parametros, ParametrosPlanilla.RentaTramo2Porc, fecha);
			var t1Piso = Obtener(parametros, ParametrosPlanilla.RentaTramo1Piso, fecha);
			var t1Porc = Obtener(parametros, ParametrosPlanilla.RentaTramo1Porc, fecha);

			decimal renta = 0m;
			if (bruto > t4Piso) { renta += (bruto - t4Piso) * (t4Porc / 100m); bruto = t4Piso; }
			if (bruto > t3Piso) { renta += (bruto - t3Piso) * (t3Porc / 100m); bruto = t3Piso; }
			if (bruto > t2Piso) { renta += (bruto - t2Piso) * (t2Porc / 100m); bruto = t2Piso; }
			if (bruto > t1Piso) { renta += (bruto - t1Piso) * (t1Porc / 100m); }
			renta = Math.Round(renta, 2);

			return (ccss, renta, ccss + renta);
		}

		// Un parámetro que falta NO se reemplaza por un default silencioso: una planilla con un
		// parámetro faltante debe fallar ruidosamente, no devolver un número mal.
		private static decimal Obtener(
			IReadOnlyDictionary<string, decimal> parametros, string nombre, DateTime fecha)
		{
			if (!parametros.TryGetValue(nombre, out var valor))
				throw new InvalidOperationException(
					$"No hay una versión vigente del parámetro '{nombre}' para la fecha " +
					$"{fecha:dd/MM/yyyy}. Configúrelo en Administración > Parámetros de planilla.");

			return valor;
		}

		private static decimal CalcularDeduccionesInternas(
			List<EmpleadoDeduccionAD> deducciones, decimal bruto)
		{
			decimal total = 0m;
			foreach (var ed in deducciones)
			{
				if (ed.Deduccion == null) continue;
				if (ed.Deduccion.EsPorcentaje && ed.Deduccion.Porcentaje.HasValue)
					total += Math.Round(bruto * (ed.Deduccion.Porcentaje.Value / 100m), 2);
				else if (!ed.Deduccion.EsPorcentaje && ed.Deduccion.Monto.HasValue)
					total += ed.Deduccion.Monto.Value;
			}
			return total;
		}

		private async Task<(
		decimal diasIncapacidad,
		decimal montoDias,
		decimal pagoPatronal,
		decimal rebajo,
		string? detalle)>
		CalcularAjusteIncapacidades(
			int idEmpleado,
			decimal salarioMensual,
			PlanillaPeriodoAD periodo,
			IReadOnlyDictionary<string, decimal> parametros)
		{
			List<IncapacidadAD> incapacidades =
				await _incapacidadLogica.ObtenerPorPeriodo(
					idEmpleado,
					periodo.FechaInicio,
					periodo.FechaFin);

			if (incapacidades.Count == 0)
			{
				return (0, 0m, 0m, 0m, null);
			}

			decimal salarioDiario =
				Math.Round(salarioMensual / 30m, 2);

			decimal totalDias = 0m;
			decimal totalMontoDias = 0m;
			decimal totalPagoPatronal = 0m;

			List<string> detalles = new();

			foreach (IncapacidadAD incapacidad in incapacidades)
			{
				decimal diasDentroPeriodo =
					_incapacidadLogica.CalcularDiasDentroPeriodo(
						incapacidad,
						periodo.FechaInicio,
						periodo.FechaFin);

				if (diasDentroPeriodo <= 0)
					continue;

				string parametroDias;
				string parametroPorcentaje;

				if (incapacidad.TipoIncapacidad ==
					TiposIncapacidad.Maternidad)
				{
					parametroDias =
						ParametrosPlanilla
							.IncapacidadMaternidadDiasPatrono;

					parametroPorcentaje =
						ParametrosPlanilla
							.IncapacidadMaternidadPorcPatrono;
				}
				else if (string.Equals(
					incapacidad.EntidadEmisora,
					"INS",
					StringComparison.OrdinalIgnoreCase))
				{
					parametroDias =
						ParametrosPlanilla
							.IncapacidadINSDiasPatrono;

					parametroPorcentaje =
						ParametrosPlanilla
							.IncapacidadINSPorcPatrono;
				}
				else
				{
					parametroDias =
						ParametrosPlanilla
							.IncapacidadCCSSDiasPatrono;

					parametroPorcentaje =
						ParametrosPlanilla
							.IncapacidadCCSSPorcPatrono;
				}

				decimal maximoDiasPatrono =
					Obtener(
						parametros,
						parametroDias,
						periodo.FechaInicio);

				decimal porcentajePatrono =
					Obtener(
						parametros,
						parametroPorcentaje,
						periodo.FechaInicio);

				if (maximoDiasPatrono < 0)
				{
					throw new InvalidOperationException(
						$"El parámetro '{parametroDias}' no puede ser negativo.");
				}

				if (porcentajePatrono < 0 ||
					porcentajePatrono > 100)
				{
					throw new InvalidOperationException(
						$"El parámetro '{parametroPorcentaje}' debe estar entre 0 y 100.");
				}

				// Los días patronales se cuentan desde el inicio real
				// de la incapacidad, no desde el inicio de cada planilla.
				int diasPatronoConfigurados =
					(int)Math.Floor(maximoDiasPatrono);

				int diasPatronoEnPeriodo = 0;

				if (diasPatronoConfigurados > 0)
				{
					DateTime finCoberturaPatronal =
						incapacidad.FechaInicio.Date
							.AddDays(diasPatronoConfigurados - 1);

					DateTime inicioCoincidencia =
						incapacidad.FechaInicio.Date >
						periodo.FechaInicio.Date
							? incapacidad.FechaInicio.Date
							: periodo.FechaInicio.Date;

					DateTime finCoincidencia =
						finCoberturaPatronal <
						periodo.FechaFin.Date
							? finCoberturaPatronal
							: periodo.FechaFin.Date;

					if (finCoincidencia >= inicioCoincidencia)
					{
						diasPatronoEnPeriodo =
							(finCoincidencia -
							 inicioCoincidencia).Days + 1;
					}
				}

				decimal montoDias =
					Math.Round(
						salarioDiario * diasDentroPeriodo,
						2);

				decimal pagoPatronal =
					Math.Round(
						salarioDiario *
						diasPatronoEnPeriodo *
						(porcentajePatrono / 100m),
						2);

				totalDias += diasDentroPeriodo;
				totalMontoDias += montoDias;
				totalPagoPatronal += pagoPatronal;

				detalles.Add(
					$"{incapacidad.TipoIncapacidad}: " +
					$"{diasDentroPeriodo} día(s)");
			}

			decimal rebajo =
				Math.Max(
					0m,
					Math.Round(
						totalMontoDias - totalPagoPatronal,
						2));

			return (
				totalDias,
				Math.Round(totalMontoDias, 2),
				Math.Round(totalPagoPatronal, 2),
				rebajo,
				string.Join("; ", detalles));
		}
	}
}