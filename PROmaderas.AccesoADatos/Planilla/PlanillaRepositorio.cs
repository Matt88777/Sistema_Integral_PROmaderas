using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos.Auditoria;

namespace PROmaderas.AccesoADatos.Planilla
{
    public class PlanillaRepositorio : IPlanillaRepositorio
    {
        private readonly Contexto _contexto;

        public PlanillaRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<List<PlanillaPeriodoAD>> ObtenerPeriodos()
        {
            return await _contexto.PlanillaPeriodos
                .Include(p => p.Detalles)
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }

        public async Task<PlanillaPeriodoAD?> ObtenerPeriodoPorId(int id)
        {
            return await _contexto.PlanillaPeriodos
                .Include(p => p.Detalles!)
                    .ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(p => p.IdPlanillaPeriodo == id);
        }

        public async Task<PlanillaPeriodoAD> CrearPeriodo(PlanillaPeriodoAD periodo)
        {
            periodo.Estado = "Borrador";
            periodo.FechaCreacion = DateTime.Now;

            _contexto.PlanillaPeriodos.Add(periodo);
            await _contexto.SaveChangesAsync();

            return periodo;
        }

        public async Task CambiarEstadoPeriodo(int id, string nuevoEstado, ContextoAuditoria auditoria)
        {
            var periodo = await _contexto.PlanillaPeriodos.FirstOrDefaultAsync(p => p.IdPlanillaPeriodo == id);
            if (periodo == null)
                throw new Exception($"No se encontró el período con ID {id}.");

            var estadoAnterior = periodo.Estado;
            periodo.Estado = nuevoEstado;

            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                "PlanillaPeriodo",
                periodo.IdPlanillaPeriodo,
                auditoria,
                new { Estado = estadoAnterior },
                new { Estado = nuevoEstado }));

            await _contexto.SaveChangesAsync();
        }

        public async Task EliminarPeriodo(int id)
        {
            var periodo = await _contexto.PlanillaPeriodos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.IdPlanillaPeriodo == id);

            if (periodo == null) return;

            if (periodo.Detalles != null && periodo.Detalles.Any())
                _contexto.PlanillaDetallesFinancieros.RemoveRange(periodo.Detalles);

            _contexto.PlanillaPeriodos.Remove(periodo);
            await _contexto.SaveChangesAsync();
        }

        public async Task<PlanillaDetalleFinancieroAD> AgregarDetalle(PlanillaDetalleFinancieroAD detalle, ContextoAuditoria auditoria)
        {
            var yaExiste = await _contexto.PlanillaDetallesFinancieros
                .AnyAsync(d => d.IdPlanillaPeriodo == detalle.IdPlanillaPeriodo && d.IdEmpleado == detalle.IdEmpleado);

            if (yaExiste)
                throw new Exception("Este empleado ya tiene horas registradas en este período.");

            _contexto.PlanillaDetallesFinancieros.Add(detalle);

            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                "PlanillaDetalle",
                null,
                auditoria,
                new { },
                new
                {
                    detalle.IdPlanillaPeriodo,
                    detalle.IdEmpleado,
                    detalle.HorasOrdinarias,
                    detalle.HorasExtra,
                    detalle.SalarioBruto
                }));

            await _contexto.SaveChangesAsync();
            return detalle;
        }
        public async Task<PlanillaDetalleFinancieroAD> ActualizarDetalle(int idDetalle, decimal horasOrdinarias, decimal horasExtra, decimal salarioBase, decimal montoHorasExtra, decimal salarioBruto, ContextoAuditoria auditoria)
        {
            var detalle = await _contexto.PlanillaDetallesFinancieros.FindAsync(idDetalle);
            if (detalle == null)
                throw new Exception("No se encontró el registro de horas a editar.");

            var valoresAnteriores = new
            {
                detalle.HorasOrdinarias,
                detalle.HorasExtra,
                detalle.SalarioBase,
                detalle.MontoHorasExtra,
                detalle.SalarioBruto
            };

            detalle.HorasOrdinarias = horasOrdinarias;
            detalle.HorasExtra = horasExtra;
            detalle.SalarioBase = salarioBase;
            detalle.MontoHorasExtra = montoHorasExtra;
            detalle.SalarioBruto = salarioBruto;
            detalle.SalarioNeto = salarioBruto - detalle.TotalDeducciones;

            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                "PlanillaDetalle",
                detalle.IdPlanillaDetalle,
                auditoria,
                valoresAnteriores,
                new { detalle.HorasOrdinarias, detalle.HorasExtra, detalle.SalarioBase, detalle.MontoHorasExtra, detalle.SalarioBruto }));

            await _contexto.SaveChangesAsync();
            return detalle;
        }
        public async Task EliminarDetalle(int idDetalle)
        {
            var detalle = await _contexto.PlanillaDetallesFinancieros.FindAsync(idDetalle);
            if (detalle != null)
            {
                _contexto.PlanillaDetallesFinancieros.Remove(detalle);
                await _contexto.SaveChangesAsync();
            }
        }

        public async Task<List<EmpleadoAD>> ObtenerEmpleadosActivos()
        {
            return await _contexto.Empleados
                .Where(e => e.Estado == true)
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }
    }
}