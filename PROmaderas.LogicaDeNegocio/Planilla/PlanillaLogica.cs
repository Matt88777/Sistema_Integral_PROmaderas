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

		private static readonly string[] EstadosValidos =
            { "Borrador", "Revisada", "Aprobada", "Pagada" };

		public PlanillaLogica(
	IPlanillaRepositorio repo,
	IParametroPlanillaRepositorio parametros,
	IPolizaINSLogica polizaINSLogica)
		{
			_repo = repo;
			_parametros = parametros;
			_polizaINSLogica = polizaINSLogica;
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

            var (salarioBase, montoExtra, bruto) = Calcular(vm.SalarioMensual, vm.HorasExtra, p, fecha);
            var (ccss, renta, totalDed) = CalcularDeducciones(bruto, p, fecha);

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
				AdvertenciaPolizaINS = advertenciaPoliza

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

            var (salarioBase, montoExtra, bruto) = Calcular(salarioMensual, horasExtra, p, fecha);
            var (ccss, renta, totalDed) = CalcularDeducciones(bruto, p, fecha);

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

			await _repo.ActualizarDetalle(detalle);
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
    }
}