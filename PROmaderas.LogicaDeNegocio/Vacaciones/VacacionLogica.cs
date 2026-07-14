using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Vacaciones
{
    public class VacacionLogica : IVacacionLogica
    {
        private readonly IVacacionRepositorio _repositorio;

        // El mismo tipo que inyecta PlanillaLogica: el repositorio de parámetros, no su lógica.
        private readonly IParametroPlanillaRepositorio _parametros;

        public VacacionLogica(IVacacionRepositorio repositorio, IParametroPlanillaRepositorio parametros)
        {
            _repositorio = repositorio;
            _parametros = parametros;
        }

        // ── Pantallas ─────────────────────────────────────────────────────────

        public async Task<List<SaldoVacacionesAD>> ObtenerListado()
        {
            // El parámetro se resuelve UNA vez para todo el listado, no una vez por empleado.
            var diasPorMes = await ObtenerDiasVacacionesPorMes();

            var empleados = await _repositorio.ObtenerEmpleadosActivos();
            var disfrutados = await _repositorio.ObtenerDisfrutadosPorEmpleado();   // UN query

            return empleados
                .Select(e => ArmarSaldo(
                    e,
                    disfrutados.TryGetValue(e.IdEmpleado ?? 0, out var d) ? d : 0m,
                    diasPorMes,
                    DateTime.Today))   // el listado siempre corta a hoy
                .OrderBy(s => s.NombreCompleto)
                .ToList();
        }

        // El saldo de la pantalla de Vacaciones: corta a hoy. Se conserva idéntico delegando en
        // la sobrecarga con fecha de corte.
        public async Task<SaldoVacacionesAD> ObtenerSaldo(int idEmpleado)
            => await ObtenerSaldo(idEmpleado, DateTime.Today);

        // PLA-HU-017: la liquidación necesita el saldo A LA FECHA DE SALIDA. Si los meses
        // trabajados se contaran contra hoy, a alguien que salió en marzo se le pagarían
        // vacaciones que nunca acumuló.
        public async Task<SaldoVacacionesAD> ObtenerSaldo(int idEmpleado, DateTime fechaCorte)
        {
            var empleado = await _repositorio.ObtenerEmpleadoPorId(idEmpleado)
                ?? throw new ArgumentException("El empleado no existe.");

            var diasPorMes = await ObtenerDiasVacacionesPorMes();
            var disfrutadas = await _repositorio.ObtenerDiasDisfrutados(idEmpleado);

            return ArmarSaldo(empleado, disfrutadas, diasPorMes, fechaCorte);
        }

        public async Task<List<VacacionAD>> ObtenerHistorial(int idEmpleado)
            => await _repositorio.ObtenerPorEmpleado(idEmpleado);

		// PLA-HU-013: vacaciones disfrutadas dentro
		// de un periodo de planilla.
		public async Task<List<VacacionAD>> ObtenerPorPeriodo(
			int idEmpleado,
			DateTime fechaInicio,
			DateTime fechaFin)
		{
			if (idEmpleado <= 0)
			{
				throw new ArgumentException(
					"El empleado seleccionado no es válido.");
			}

			if (fechaInicio == default ||
				fechaFin == default)
			{
				throw new ArgumentException(
					"Las fechas del periodo son obligatorias.");
			}

			if (fechaFin.Date < fechaInicio.Date)
			{
				throw new ArgumentException(
					"La fecha final del periodo no puede ser anterior a la fecha inicial.");
			}

			return await _repositorio.ObtenerPorPeriodo(
				idEmpleado,
				fechaInicio.Date,
				fechaFin.Date);
		}

		public decimal CalcularDiasDentroPeriodo(
			VacacionAD vacacion,
			DateTime fechaInicio,
			DateTime fechaFin)
		{
			if (vacacion == null)
				throw new ArgumentNullException(
					nameof(vacacion));

			if (fechaFin.Date < fechaInicio.Date)
			{
				throw new ArgumentException(
					"La fecha final del periodo no puede ser anterior a la fecha inicial.");
			}

			DateTime inicioReal =
				vacacion.FechaInicio.Date >
				fechaInicio.Date
					? vacacion.FechaInicio.Date
					: fechaInicio.Date;

			DateTime finReal =
				vacacion.FechaFin.Date <
				fechaFin.Date
					? vacacion.FechaFin.Date
					: fechaFin.Date;

			if (finReal < inicioReal)
				return 0m;

			int diasNaturalesTotales =
				(vacacion.FechaFin.Date -
				 vacacion.FechaInicio.Date).Days + 1;

			int diasNaturalesCoincidentes =
				(finReal - inicioReal).Days + 1;

			if (diasNaturalesTotales <= 0)
				return 0m;

			// Si todo el registro está dentro del periodo,
			// se conserva exactamente el valor digitado.
			if (diasNaturalesCoincidentes ==
				diasNaturalesTotales)
			{
				return vacacion.Dias;
			}

			decimal proporcion =
				diasNaturalesCoincidentes /
				(decimal)diasNaturalesTotales;

			return Math.Round(
				vacacion.Dias * proporcion,
				2);
		}

		// ── Escrituras ────────────────────────────────────────────────────────

		public async Task Registrar(int idEmpleado, DateTime inicio, DateTime fin, decimal dias,
                                    string observacion, ContextoAuditoria auditoria)
        {
            if (fin.Date < inicio.Date)
                throw new ArgumentException(
                    "La fecha de fin no puede ser anterior a la fecha de inicio.");

            if (dias <= 0)
                throw new ArgumentException("Los días de vacaciones deben ser mayores a cero.");

            // Tira ArgumentException si el empleado no existe.
            var saldo = await ObtenerSaldo(idEmpleado);

            // BUG B: sin fecha de ingreso no hay meses trabajados, y sin meses trabajados el
            // acumulado es mentira. En el listado esto solo atenúa la fila; acá sí se bloquea,
            // porque escribir contra un saldo que no se puede calcular es peor que no escribir.
            if (!saldo.TieneFechaIngreso)
                throw new ArgumentException(
                    "El empleado no tiene fecha de ingreso registrada. " +
                    "Corríjalo en Empleados > Editar antes de registrarle vacaciones.");

            if (saldo.Saldo - dias < 0)
                throw new ArgumentException(
                    $"El empleado tiene {saldo.Saldo:N2} día(s) de saldo disponible y se están " +
                    $"registrando {dias:N2} día(s). No se puede dejar el saldo en negativo.");

            var vacacion = new VacacionAD
            {
                IdEmpleado = idEmpleado,
                FechaInicio = inicio.Date,
                FechaFin = fin.Date,

                // Los días son los que DIGITÓ el usuario: no se recalculan del rango. Un rango
                // de 7 días naturales puede ser de 5 días de vacaciones, y existen medios días.
                Dias = dias,

                // SIEMPRE explícito. La columna no tiene CHECK y su DEFAULT ('Registrada') no
                // es un estado del dominio: si se dejara disparar, la fila no sumaría a las
                // disfrutadas y el saldo saldría inflado en silencio.
                Estado = EstadosVacacion.Disfrutada,

                Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim()
            };

            await _repositorio.Crear(vacacion, auditoria);
        }

        public async Task Anular(int idVacacion, string motivo, ContextoAuditoria auditoria)
        {
            var motivoLimpio = ValidarMotivo(motivo);

            var vacacion = await _repositorio.ObtenerPorId(idVacacion)
                ?? throw new ArgumentException("La vacación no existe.");

            if (vacacion.Estado == EstadosVacacion.Anulada)
                throw new ArgumentException("Esta vacación ya está anulada.");

            await _repositorio.Anular(idVacacion, motivoLimpio, auditoria);
        }

        // ── Cálculo ───────────────────────────────────────────────────────────

        // 🪤 EXCEPCIÓN DELIBERADA a la regla de PLA-HU-019 ("los parámetros se resuelven a la
        // fecha del período, no a la de hoy").
        //
        // Acá el parámetro se resuelve UNA SOLA VEZ a DateTime.Today y se MULTIPLICA por los
        // meses trabajados. NO se resuelve mes a mes a lo largo de la historia del empleado.
        //
        // Razón: todas las versiones de parámetros arrancan su vigencia el 2026-01-01. Un
        // empleado que ingresó en 2021 no tiene parámetro vigente en 2021, así que resolver
        // mes a mes lanzaría InvalidOperationException para CASI TODOS los empleados y la
        // pantalla no abriría nunca. Las pantallas de administración se resuelven a hoy; el
        // cálculo de planilla (que sí es histórico y auditable) sigue resolviendo por período.
        private async Task<decimal> ObtenerDiasVacacionesPorMes()
        {
            var hoy = DateTime.Today;

            var valor = await _parametros.ObtenerValorVigente(ParametrosPlanilla.DiasVacacionesPorMes, hoy);

            // Un parámetro que falta NO se reemplaza por un default silencioso: mismo criterio
            // que el helper Obtener de PlanillaLogica. Un saldo mal calculado es peor que una
            // pantalla que no abre y dice por qué.
            if (valor == null)
                throw new InvalidOperationException(
                    $"No hay una versión vigente del parámetro '{ParametrosPlanilla.DiasVacacionesPorMes}' " +
                    $"para la fecha {hoy:dd/MM/yyyy}. Configúrelo en Administración > Parámetros de planilla.");

            return valor.Value;
        }

        // fechaCorte: hasta cuándo se cuentan los meses trabajados. La pantalla de Vacaciones
        // pasa DateTime.Today; la liquidación pasa la fecha de salida (PLA-HU-017).
        private static SaldoVacacionesAD ArmarSaldo(EmpleadoAD empleado, decimal disfrutadas,
                                                    decimal diasPorMes, DateTime fechaCorte)
        {
            var tieneFechaIngreso = empleado.FechaIngreso.HasValue;

            var meses = tieneFechaIngreso
                ? CalcularMesesCompletos(empleado.FechaIngreso!.Value, fechaCorte)
                : 0;

            // BUG B: sin fecha de ingreso no hay acumulado que valga. Se deja en 0 y la vista
            // pinta "—" mirando TieneFechaIngreso: mostrar un número inventado sería peor.
            var acumuladas = tieneFechaIngreso
                ? empleado.SaldoVacacionesInicial + (meses * diasPorMes)
                : 0m;

            var nombreCompleto = string.Join(" ", new[]
            {
                empleado.Nombre,
                empleado.PrimerApellido,
                empleado.SegundoApellido
            }.Where(p => !string.IsNullOrWhiteSpace(p)));

            return new SaldoVacacionesAD
            {
                IdEmpleado = empleado.IdEmpleado ?? 0,
                NombreCompleto = nombreCompleto,
                Cedula = empleado.Cedula,
                FechaIngreso = empleado.FechaIngreso,
                SaldoInicial = empleado.SaldoVacacionesInicial,
                MesesTrabajados = meses,
                Acumuladas = acumuladas,
                Disfrutadas = disfrutadas,
                Saldo = tieneFechaIngreso ? acumuladas - disfrutadas : 0m,
                TieneFechaIngreso = tieneFechaIngreso
            };
        }

        // MESES COMPLETOS (floor). El mes se cumple cuando llega el MISMO DÍA DEL MES que la
        // fecha de ingreso: si ingresó el 15/03/2025 y hoy es 12/07/2026, son 15 meses (no 16),
        // porque el mes 16 se cumple hasta el 15/07/2026.
        private static int CalcularMesesCompletos(DateTime ingreso, DateTime hasta)
        {
            var desde = ingreso.Date;
            var fin = hasta.Date;

            // Fecha de ingreso a futuro (dato sucio): 0 meses, no un número negativo.
            if (fin < desde) return 0;

            var meses = ((fin.Year - desde.Year) * 12) + (fin.Month - desde.Month);

            // Todavía no llega el día del mes en que se cumple: el mes en curso no cuenta.
            if (fin.Day < desde.Day) meses--;

            return meses < 0 ? 0 : meses;
        }

        // ── Validaciones ──────────────────────────────────────────────────────

        // El motivo es la justificación de la anulación: sin él no se guarda nada en la bitácora.
        private static string ValidarMotivo(string motivo)
        {
            var limpio = (motivo ?? string.Empty).Trim();

            if (limpio.Length == 0)
                throw new ArgumentException("Debe indicar el motivo de la anulación.");

            return limpio;
        }
    }
}
