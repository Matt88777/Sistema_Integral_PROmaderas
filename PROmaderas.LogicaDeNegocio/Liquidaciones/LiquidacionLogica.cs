using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Liquidaciones
{
    public class LiquidacionLogica : ILiquidacionLogica
    {
        // 🪤 DEUDA CONOCIDA: el divisor para sacar el salario de un día DEBERÍA ser un parámetro
        // de planilla versionado ('DiasMes'), como HorasMes. Va hardcodeado porque sembrar un
        // parámetro nuevo obliga a tocar el script de BD y a que todo el equipo lo vuelva a
        // correr. Si algún día cambia el criterio (p.ej. 30.42 = 365/12), esto se convierte en
        // parámetro y este comentario se borra.
        private const decimal DiasMes = 30m;

        // Los años se sacan en días / 365.25 para no perder los bisiestos: con 365 plano, alguien
        // con 8 años exactos daría 8.05 y se le reconocerían años que no trabajó.
        private const decimal DiasAnio = 365.25m;

        private readonly ILiquidacionRepositorio _repositorio;
        private readonly IParametroPlanillaLogica _parametros;
        private readonly IVacacionLogica _vacaciones;

        // El REPOSITORIO de aguinaldo, no su lógica: IAguinaldoLogica tiene la ventana dic→dic
        // hardcodeada y devuelve todos los empleados. Acá se necesita un rango arbitrario que
        // termina en la fecha de salida, y de un solo empleado.
        private readonly IAguinaldoRepositorio _aguinaldo;

        public LiquidacionLogica(ILiquidacionRepositorio repositorio,
                                 IParametroPlanillaLogica parametros,
                                 IVacacionLogica vacaciones,
                                 IAguinaldoRepositorio aguinaldo)
        {
            _repositorio = repositorio;
            _parametros = parametros;
            _vacaciones = vacaciones;
            _aguinaldo = aguinaldo;
        }

        // ── Pantallas ─────────────────────────────────────────────────────────

        public async Task<List<LiquidacionAD>> ObtenerTodas()
            => await _repositorio.ObtenerTodas();

        public async Task<LiquidacionAD?> ObtenerPorId(int id)
            => await _repositorio.ObtenerPorId(id);

        public async Task<List<EmpleadoAD>> ObtenerEmpleadosLiquidables()
            => await _repositorio.ObtenerEmpleadosActivos();

        public async Task<Dictionary<int, string>> ObtenerNombresEmpleados()
            => await _repositorio.ObtenerNombresEmpleados();

        // Escenario 2: el desglose de una liquidación YA EMITIDA se reconstruye desde los montos
        // CONGELADOS en la fila, no se recalcula. Si los parámetros cambiaron o al empleado le
        // corrigieron la fecha de ingreso, este documento tiene que seguir diciendo exactamente
        // lo que se pagó. Recalcular acá sería falsificar evidencia.
        public async Task<LiquidacionCalculoAD?> ObtenerDesglose(int id)
        {
            var l = await _repositorio.ObtenerPorId(id);
            if (l == null) return null;

            var empleado = await _repositorio.ObtenerEmpleadoPorId(l.IdEmpleado);
            var salarioDia = l.SalarioPromedio / DiasMes;

            // Los flags se derivan del motivo congelado en la fila: son los mismos con los que
            // se calculó.
            var generaPreaviso = MotivosSalida.GeneraPreaviso(l.MotivoSalida);
            var generaCesantia = MotivosSalida.GeneraCesantia(l.MotivoSalida);

            return new LiquidacionCalculoAD
            {
                IdLiquidacion = l.IdLiquidacion,
                EstadoLiquidacion = l.Estado,

                IdEmpleado = l.IdEmpleado,
                NombreEmpleado = NombreCompleto(empleado),
                Cedula = empleado?.Cedula,

                FechaIngreso = l.FechaIngreso,
                FechaSalida = l.FechaSalida,
                MotivoSalida = l.MotivoSalida,

                AniosLaborados = l.AniosLaborados,
                SalarioPromedio = l.SalarioPromedio,
                SalarioDia = Math.Round(salarioDia, 2),

                DiasVacacionesPendientes = l.DiasVacacionesPendientes,
                MontoVacaciones = l.MontoVacaciones,

                AguinaldoDesde = new DateTime(l.FechaSalida.Year - 1, 12, 1),
                AguinaldoHasta = l.FechaSalida,
                // Se despeja del monto congelado: brutos = aguinaldo * 12. No se vuelve a
                // consultar la planilla, que pudo cambiar.
                AguinaldoBrutosDevengados = Math.Round(l.MontoAguinaldoProporcional * 12m, 2),
                MontoAguinaldoProporcional = l.MontoAguinaldoProporcional,

                GeneraPreaviso = generaPreaviso,
                PreavisoDias = salarioDia > 0 ? Math.Round(l.MontoPreaviso / salarioDia, 2) : 0m,
                MontoPreaviso = l.MontoPreaviso,

                GeneraCesantia = generaCesantia,
                MontoCesantia = l.MontoCesantia,

                OtrosMontos = l.OtrosMontos
            };
        }

        // ── Cálculo (PREVIEW: no escribe nada) ────────────────────────────────

        public async Task<LiquidacionCalculoAD> Calcular(int idEmpleado, DateTime fechaSalida,
                                                         string motivo)
        {
            var empleado = await Validar(idEmpleado, fechaSalida, motivo);
            return await CalcularDesglose(empleado, fechaSalida.Date, motivo);
        }

        // ── Escrituras ────────────────────────────────────────────────────────

        public async Task Guardar(int idEmpleado, DateTime fechaSalida, string motivo,
                                  decimal otrosMontos, string? observacion,
                                  ContextoAuditoria auditoria)
        {
            if (otrosMontos < 0)
                throw new ArgumentException("Los otros montos no pueden ser negativos.");

            // RECALCULA todo desde cero. Los montos que venga a mandar el formulario se ignoran:
            // si se confiara en ellos, cualquiera podría postear un total inventado.
            var calculo = await Calcular(idEmpleado, fechaSalida, motivo);
            calculo.OtrosMontos = otrosMontos;

            // Fallback a admin (IdUsuario = 1), igual que Factura.IdUsuarioEmisor: la FK es
            // NOT NULL y el correo de Identity puede no tener espejo en dbo.Usuario.
            var idUsuario = await _repositorio.ObtenerIdUsuarioPorCorreo(auditoria.Email) ?? 1;

            var liquidacion = new LiquidacionAD
            {
                IdEmpleado = calculo.IdEmpleado,
                FechaCalculo = DateTime.Now,

                // Congelados: la liquidación es evidencia y no puede moverse si mañana editan
                // al empleado o los parámetros de planilla.
                FechaIngreso = calculo.FechaIngreso,
                FechaSalida = calculo.FechaSalida,
                MotivoSalida = calculo.MotivoSalida,
                AniosLaborados = calculo.AniosLaborados,
                SalarioPromedio = calculo.SalarioPromedio,

                DiasVacacionesPendientes = calculo.DiasVacacionesPendientes,
                MontoVacaciones = calculo.MontoVacaciones,
                MontoAguinaldoProporcional = calculo.MontoAguinaldoProporcional,
                MontoPreaviso = calculo.MontoPreaviso,     // 0 si el motivo no lo genera
                MontoCesantia = calculo.MontoCesantia,     // 0 si el motivo no la genera
                OtrosMontos = calculo.OtrosMontos,
                TotalLiquidacion = Math.Round(calculo.Total, 2),

                // SIEMPRE explícito, aunque la columna tenga DEFAULT ('Calculada').
                Estado = EstadosLiquidacion.Calculada,
                Activa = true,

                Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim(),
                IdUsuarioRegistro = idUsuario
            };

            await _repositorio.Guardar(liquidacion, auditoria);
        }

        public async Task Anular(int idLiquidacion, string motivo, ContextoAuditoria auditoria)
        {
            var motivoLimpio = ValidarMotivoAnulacion(motivo);

            var liquidacion = await _repositorio.ObtenerPorId(idLiquidacion)
                ?? throw new ArgumentException("La liquidación no existe.");

            if (!liquidacion.Activa || liquidacion.Estado == EstadosLiquidacion.Anulada)
                throw new ArgumentException("Esta liquidación ya está anulada.");

            await _repositorio.Anular(idLiquidacion, motivoLimpio, auditoria);
        }

        // ── El cálculo ────────────────────────────────────────────────────────

        private async Task<LiquidacionCalculoAD> CalcularDesglose(EmpleadoAD empleado,
                                                                  DateTime fechaSalida,
                                                                  string motivo)
        {
            var idEmpleado = empleado.IdEmpleado ?? 0;
            var fechaIngreso = empleado.FechaIngreso!.Value.Date;
            var salarioMensual = empleado.SalarioBase!.Value;

            var salarioDia = salarioMensual / DiasMes;

            // Se redondea UNA vez y se usa el redondeado en todos lados, para que el número que
            // se guarda sea exactamente el que se usó al calcular (auditable).
            var anios = Math.Round(
                (decimal)(fechaSalida - fechaIngreso).TotalDays / DiasAnio, 2);

            // 🪤 Los 3 parámetros se resuelven a la FECHA DE SALIDA, en UN solo query.
            // NO a la fecha de ingreso: todos los parámetros arrancan su vigencia el 2026-01-01,
            // así que resolverlos contra un ingreso de 2024 no encontraría versión vigente y
            // reventaría siempre.
            var p = await _parametros.ObtenerVigentes(fechaSalida);

            var preavisoDias = Obtener(p, ParametrosPlanilla.PreavisoDias, fechaSalida);
            var cesantiaDiasPorAnio = Obtener(p, ParametrosPlanilla.CesantiaDiasPorAnio, fechaSalida);
            var cesantiaTopeAnios = Obtener(p, ParametrosPlanilla.CesantiaTopeAnios, fechaSalida);

            // ── Rubro 1: vacaciones pendientes. SIEMPRE se pagan, con cualquier motivo:
            // son salario ya devengado, no una indemnización.
            // El saldo se corta a la FECHA DE SALIDA, no a hoy (PLA-HU-017).
            var saldo = await _vacaciones.ObtenerSaldo(idEmpleado, fechaSalida);
            var diasVacaciones = saldo.Saldo;
            var montoVacaciones = Math.Round(diasVacaciones * salarioDia, 2);

            // ── Rubro 2: aguinaldo proporcional. SIEMPRE se paga.
            var (aguinaldoDesde, aguinaldoHasta, brutos, montoAguinaldo) =
                await CalcularAguinaldoProporcional(empleado, fechaSalida);

            // ── Rubro 3: preaviso. Solo si el motivo lo genera (Escenario 3).
            var generaPreaviso = MotivosSalida.GeneraPreaviso(motivo);

            // 🪤 preavisoDias NO se multiplica por los años, pese a que la fila de la BD se llame
            // "PreavisoDiasPorAnio". En Costa Rica el preaviso topa en 1 mes: con 2 años o con 20,
            // son 30 días. Multiplicarlo por la antigüedad pagaría múltiplos de más.
            var montoPreaviso = generaPreaviso
                ? Math.Round(preavisoDias * salarioDia, 2)
                : 0m;

            // ── Rubro 4: cesantía. Solo si el motivo la genera (Escenario 3).
            var generaCesantia = MotivosSalida.GeneraCesantia(motivo);

            // La cesantía sí se multiplica por los años, pero topados: más allá del tope no se
            // reconoce antigüedad.
            var aniosReconocidos = Math.Min(anios, cesantiaTopeAnios);

            // ⚠️ SIMPLIFICACIÓN CONSCIENTE: el art. 29 del Código de Trabajo de CR usa una tabla
            // ESCALONADA (año 1 = 19.5 días, año 2 = 20, año 3 = 20.5 ...), no un promedio plano.
            // Acá se usa un único 'CesantiaDiasPorAnio' porque es lo que el diseño aprobado sembró
            // en la BD. Para casos reales de muchos años, el monto difiere del legal.
            var montoCesantia = generaCesantia
                ? Math.Round(cesantiaDiasPorAnio * aniosReconocidos * salarioDia, 2)
                : 0m;

            return new LiquidacionCalculoAD
            {
                IdEmpleado = idEmpleado,
                NombreEmpleado = NombreCompleto(empleado),
                Cedula = empleado.Cedula,

                FechaIngreso = fechaIngreso,
                FechaSalida = fechaSalida,
                MotivoSalida = motivo,

                AniosLaborados = anios,
                SalarioPromedio = salarioMensual,
                SalarioDia = Math.Round(salarioDia, 2),

                DiasVacacionesPendientes = diasVacaciones,
                MontoVacaciones = montoVacaciones,

                AguinaldoDesde = aguinaldoDesde,
                AguinaldoHasta = aguinaldoHasta,
                AguinaldoBrutosDevengados = brutos,
                MontoAguinaldoProporcional = montoAguinaldo,

                GeneraPreaviso = generaPreaviso,
                PreavisoDias = preavisoDias,
                MontoPreaviso = montoPreaviso,

                GeneraCesantia = generaCesantia,
                CesantiaDiasPorAnio = cesantiaDiasPorAnio,
                CesantiaTopeAnios = cesantiaTopeAnios,
                CesantiaAniosReconocidos = aniosReconocidos,
                MontoCesantia = montoCesantia,

                OtrosMontos = 0m   // lo asigna quien llama; el Total se recalcula solo
            };
        }

        // El aguinaldo se calcula sobre los SALARIOS BRUTOS DEVENGADOS (las planillas reales),
        // no sobre SalarioBase: así lo manda la ley y así lo hace el módulo de Aguinaldo que ya
        // existe. Una persona con horas extra cobra más aguinaldo que su salario base.
        private async Task<(DateTime desde, DateTime hasta, decimal brutos, decimal monto)>
            CalcularAguinaldoProporcional(EmpleadoAD empleado, DateTime fechaSalida)
        {
            var desde = new DateTime(fechaSalida.Year - 1, 12, 1);

            // ⚠️ IAguinaldoRepositorio.ObtenerDetallesPorPeriodo filtra con
            // 'Periodo.FechaInicio >= desde && Periodo.FechaInicio < hasta': el límite superior es
            // EXCLUSIVO. Si se pasara fechaSalida tal cual, un período que ARRANCA justo el día de
            // la salida quedaría fuera y su bruto no contaría. Se pasa fechaSalida + 1 día para
            // volverlo inclusivo. El repositorio es de otro módulo y no se toca.
            var hastaExclusivo = fechaSalida.AddDays(1);

            var detalles = await _aguinaldo.ObtenerDetallesPorPeriodo(desde, hastaExclusivo);

            // El repositorio devuelve los detalles de TODOS los empleados: se filtra el nuestro.
            var delEmpleado = detalles
                .Where(d => d.IdEmpleado == (empleado.IdEmpleado ?? 0))
                .ToList();

            // 🔴 NUNCA devolver 0 en silencio. Un aguinaldo en cero por falta de planillas no es
            // "no le corresponde": es un cálculo incompleto, y acá eso es plata que la persona
            // deja de cobrar. Que falle ruidosamente y diga qué hacer.
            if (delEmpleado.Count == 0)
                throw new InvalidOperationException(
                    $"No se puede liquidar a {NombreCompleto(empleado)}: no tiene planillas " +
                    $"calculadas entre el {desde:dd/MM/yyyy} y el {fechaSalida:dd/MM/yyyy}. " +
                    "El aguinaldo proporcional se calcula sobre los salarios brutos devengados. " +
                    "Calcule las planillas del período antes de liquidar.");

            var brutos = delEmpleado.Sum(d => d.SalarioBruto);

            return (desde, fechaSalida, brutos, Math.Round(brutos / 12m, 2));
        }

        // ── Validaciones ──────────────────────────────────────────────────────

        private async Task<EmpleadoAD> Validar(int idEmpleado, DateTime fechaSalida, string motivo)
        {
            if (!MotivosSalida.EsValido(motivo))
                throw new ArgumentException(
                    $"El motivo de salida '{motivo}' no es válido. " +
                    $"Los motivos permitidos son: {string.Join(", ", MotivosSalida.Todos)}.");

            var empleado = await _repositorio.ObtenerEmpleadoPorId(idEmpleado)
                ?? throw new ArgumentException("El empleado no existe.");

            // Un empleado inactivo ya salió. Sin esto se le podría emitir una segunda liquidación
            // después de anular la primera y volver a inactivarlo por otra vía.
            if (empleado.Estado != true)
                throw new ArgumentException(
                    $"{NombreCompleto(empleado)} no está activo: no se puede liquidar a alguien " +
                    "que ya salió de la empresa.");

            if (!empleado.FechaIngreso.HasValue)
                throw new ArgumentException(
                    $"{NombreCompleto(empleado)} no tiene fecha de ingreso registrada. " +
                    "Corríjalo en Empleados > Editar: sin ella no se pueden calcular los años " +
                    "laborados ni las vacaciones acumuladas.");

            if (!empleado.SalarioBase.HasValue || empleado.SalarioBase.Value <= 0)
                throw new ArgumentException(
                    $"{NombreCompleto(empleado)} no tiene un salario base registrado. " +
                    "Corríjalo en Empleados > Editar: todos los rubros se calculan sobre él.");

            if (fechaSalida.Date < empleado.FechaIngreso.Value.Date)
                throw new ArgumentException(
                    $"La fecha de salida ({fechaSalida:dd/MM/yyyy}) no puede ser anterior a la " +
                    $"fecha de ingreso ({empleado.FechaIngreso.Value:dd/MM/yyyy}).");

            // No se liquida dos veces a la misma persona. Para rehacer una liquidación mal
            // calculada hay que anular la anterior primero.
            if (await _repositorio.TieneLiquidacionActiva(idEmpleado))
                throw new ArgumentException(
                    $"{NombreCompleto(empleado)} ya tiene una liquidación activa. " +
                    "Anúlela antes de calcular una nueva.");

            return empleado;
        }

        // Un parámetro que falta NO se reemplaza por un default silencioso: mismo criterio que el
        // helper Obtener de PlanillaLogica. Una liquidación con un parámetro asumido es plata mal
        // pagada, y nadie se enteraría.
        private static decimal Obtener(IReadOnlyDictionary<string, decimal> parametros,
                                       string nombre, DateTime fecha)
        {
            if (!parametros.TryGetValue(nombre, out var valor))
                throw new InvalidOperationException(
                    $"No hay una versión vigente del parámetro '{nombre}' para la fecha " +
                    $"{fecha:dd/MM/yyyy}. Configúrelo en Administración > Parámetros de planilla.");

            return valor;
        }

        private static string ValidarMotivoAnulacion(string motivo)
        {
            var limpio = (motivo ?? string.Empty).Trim();

            if (limpio.Length == 0)
                throw new ArgumentException("Debe indicar el motivo de la anulación.");

            return limpio;
        }

        private static string NombreCompleto(EmpleadoAD? empleado)
        {
            if (empleado == null) return "(empleado no encontrado)";

            return string.Join(" ", new[]
            {
                empleado.Nombre,
                empleado.PrimerApellido,
                empleado.SegundoApellido
            }.Where(p => !string.IsNullOrWhiteSpace(p)));
        }
    }
}
