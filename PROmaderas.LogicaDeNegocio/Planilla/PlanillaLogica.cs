using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Planilla
{
    public class PlanillaLogica : IPlanillaLogica
    {
        private readonly IPlanillaRepositorio _repo;

        private static readonly string[] EstadosValidos =
            { "Borrador", "Revisada", "Aprobada", "Pagada" };

        public PlanillaLogica(IPlanillaRepositorio repo)
        {
            _repo = repo;
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
            var (salarioBase, montoExtra, bruto) = Calcular(vm.SalarioMensual, vm.HorasExtra);

            var detalle = new PlanillaDetalleFinancieroAD
            {
                IdPlanillaPeriodo = vm.IdPlanillaPeriodo,
                IdEmpleado = vm.IdEmpleado,
                SalarioBase = salarioBase,
                HorasOrdinarias = vm.HorasOrdinarias,
                HorasExtra = vm.HorasExtra,
                MontoHorasExtra = montoExtra,
                SalarioBruto = bruto,
                TotalDeducciones = 0,
                SalarioNeto = bruto
            };

            await _repo.AgregarDetalle(detalle);
        }

        public async Task ActualizarHoras(int idDetalle, decimal salarioMensual,
            decimal horasOrdinarias, decimal horasExtra, ContextoAuditoria auditoria)
        {
            var detalle = await _repo.ObtenerDetallePorId(idDetalle)
                ?? throw new ArgumentException("Detalle no encontrado.");

            var (salarioBase, montoExtra, bruto) = Calcular(salarioMensual, horasExtra);

            detalle.SalarioBase = salarioBase;
            detalle.HorasOrdinarias = horasOrdinarias;
            detalle.HorasExtra = horasExtra;
            detalle.MontoHorasExtra = montoExtra;
            detalle.SalarioBruto = bruto;
            detalle.SalarioNeto = bruto - detalle.TotalDeducciones;

            await _repo.ActualizarDetalle(detalle);
        }

        public async Task EliminarDetalle(int idDetalle)
            => await _repo.EliminarDetalle(idDetalle);

        public async Task<List<EmpleadoAD>> ObtenerEmpleadosActivos()
            => await _repo.ObtenerEmpleadosActivos();

        private static (decimal salarioBase, decimal montoExtra, decimal bruto) Calcular(
            decimal salarioMensual, decimal horasExtra)
        {
            const decimal horasMes = 240m;
            var valorHora = salarioMensual / horasMes;
            var montoExtra = Math.Round(valorHora * 1.5m * horasExtra, 2);
            var bruto = Math.Round(salarioMensual + montoExtra, 2);
            return (salarioMensual, montoExtra, bruto);
        }
    }
}