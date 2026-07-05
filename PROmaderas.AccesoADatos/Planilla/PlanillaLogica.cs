using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Planilla
{
    public class PlanillaLogica : IPlanillaLogica
    {
        private readonly IPlanillaRepositorio _repositorio;

        // Jornada estándar utilizada para derivar el valor de la hora ordinaria
        // a partir del salario mensual (8 horas x 30 días).
        private const decimal HorasJornadaMensual = 240m;

        // Recargo legal de la hora extra en Costa Rica: tiempo y medio.
        private const decimal FactorHoraExtra = 1.5m;

        public PlanillaLogica(IPlanillaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<List<PlanillaPeriodoAD>> ObtenerPeriodos() => _repositorio.ObtenerPeriodos();

        public Task<PlanillaPeriodoAD?> ObtenerPeriodoPorId(int id) => _repositorio.ObtenerPeriodoPorId(id);

        public Task<PlanillaPeriodoAD> CrearPeriodo(PlanillaPeriodoAD periodo) => _repositorio.CrearPeriodo(periodo);

        public Task CambiarEstadoPeriodo(int id, string nuevoEstado, ContextoAuditoria auditoria) =>
            _repositorio.CambiarEstadoPeriodo(id, nuevoEstado, auditoria);

        public Task EliminarPeriodo(int id) => _repositorio.EliminarPeriodo(id);

        public Task<List<EmpleadoAD>> ObtenerEmpleadosActivos() => _repositorio.ObtenerEmpleadosActivos();

        public Task EliminarDetalle(int idDetalle) => _repositorio.EliminarDetalle(idDetalle);

        public async Task<PlanillaDetalleFinancieroAD> RegistrarHoras(PlanillaDetalleFormVM vm, ContextoAuditoria auditoria)
        {
            // PLA-HU-003: horas ordinarias -> salario base devengado (prorrateado)
            var valorHoraOrdinaria = vm.SalarioMensual / HorasJornadaMensual;
            var salarioBaseDevengado = Math.Round(valorHoraOrdinaria * vm.HorasOrdinarias, 2);

            // PLA-HU-004: horas extra pagadas a tiempo y medio
            var montoHorasExtra = Math.Round(valorHoraOrdinaria * FactorHoraExtra * vm.HorasExtra, 2);

            // PLA-HU-005: salario bruto = salario base devengado + monto de horas extra
            var salarioBruto = salarioBaseDevengado + montoHorasExtra;

            var detalle = new PlanillaDetalleFinancieroAD
            {
                IdPlanillaPeriodo = vm.IdPlanillaPeriodo,
                IdEmpleado = vm.IdEmpleado,
                HorasOrdinarias = vm.HorasOrdinarias,
                HorasExtra = vm.HorasExtra,
                SalarioBase = salarioBaseDevengado,
                MontoHorasExtra = montoHorasExtra,
                SalarioBruto = salarioBruto,
                TotalDeducciones = 0m,
                SalarioNeto = salarioBruto
            };

            return await _repositorio.AgregarDetalle(detalle, auditoria);
        }

        public async Task<PlanillaDetalleFinancieroAD> ActualizarHoras(int idPlanillaDetalle, decimal salarioMensual, decimal horasOrdinarias, decimal horasExtra, ContextoAuditoria auditoria)
        {
            var valorHoraOrdinaria = salarioMensual / HorasJornadaMensual;
            var salarioBaseDevengado = Math.Round(valorHoraOrdinaria * horasOrdinarias, 2);
            var montoHorasExtra = Math.Round(valorHoraOrdinaria * FactorHoraExtra * horasExtra, 2);
            var salarioBruto = salarioBaseDevengado + montoHorasExtra;

            return await _repositorio.ActualizarDetalle(idPlanillaDetalle, horasOrdinarias, horasExtra, salarioBaseDevengado, montoHorasExtra, salarioBruto, auditoria);
        }
    }
}