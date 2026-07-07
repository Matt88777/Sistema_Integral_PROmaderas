using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

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
            => await _contexto.PlanillaPeriodos
                .Include(p => p.Detalles)
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();

        public async Task<PlanillaPeriodoAD?> ObtenerPeriodoPorId(int id)
            => await _contexto.PlanillaPeriodos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.IdPlanillaPeriodo == id);

        public async Task<PlanillaPeriodoAD> CrearPeriodo(PlanillaPeriodoAD periodo)
        {
            periodo.FechaCreacion = DateTime.Now;
            _contexto.PlanillaPeriodos.Add(periodo);
            await _contexto.SaveChangesAsync();
            return periodo;
        }

        public async Task CambiarEstadoPeriodo(int id, string nuevoEstado, ContextoAuditoria auditoria)
        {
            var periodo = await _contexto.PlanillaPeriodos
                .FirstOrDefaultAsync(p => p.IdPlanillaPeriodo == id)
                ?? throw new ArgumentException("Período no encontrado.");

            periodo.Estado = nuevoEstado;
            await _contexto.SaveChangesAsync();
        }

        public async Task EliminarPeriodo(int id)
        {
            var periodo = await _contexto.PlanillaPeriodos
                .FirstOrDefaultAsync(p => p.IdPlanillaPeriodo == id)
                ?? throw new ArgumentException("Período no encontrado.");

            _contexto.PlanillaPeriodos.Remove(periodo);
            await _contexto.SaveChangesAsync();
        }

        public async Task<List<PlanillaDetalleFinancieroAD>> ObtenerDetallesPorPeriodo(int idPeriodo)
            => await _contexto.PlanillaDetallesFinancieros
                .Include(d => d.Empleado)
                .Where(d => d.IdPlanillaPeriodo == idPeriodo)
                .OrderBy(d => d.Empleado!.Nombre)
                .ToListAsync();

        public async Task<PlanillaDetalleFinancieroAD> AgregarDetalle(PlanillaDetalleFinancieroAD detalle)
        {
            _contexto.PlanillaDetallesFinancieros.Add(detalle);
            await _contexto.SaveChangesAsync();
            return detalle;
        }

        public async Task<PlanillaDetalleFinancieroAD?> ObtenerDetallePorId(int idDetalle)
            => await _contexto.PlanillaDetallesFinancieros
                .FirstOrDefaultAsync(d => d.IdPlanillaDetalle == idDetalle);

        public async Task ActualizarDetalle(PlanillaDetalleFinancieroAD detalle)
        {
            _contexto.PlanillaDetallesFinancieros.Update(detalle);
            await _contexto.SaveChangesAsync();
        }

        public async Task EliminarDetalle(int idDetalle)
        {
            var detalle = await _contexto.PlanillaDetallesFinancieros
                .FirstOrDefaultAsync(d => d.IdPlanillaDetalle == idDetalle)
                ?? throw new ArgumentException("Detalle no encontrado.");

            _contexto.PlanillaDetallesFinancieros.Remove(detalle);
            await _contexto.SaveChangesAsync();
        }

        public async Task<List<EmpleadoAD>> ObtenerEmpleadosActivos()
            => await _contexto.Empleados
                .Where(e => e.Estado == true)
                .OrderBy(e => e.Nombre)
                .ToListAsync();

        public async Task<List<EmpleadoDeduccionAD>> ObtenerDeduccionesActivasDeEmpleado(int idEmpleado)
            => await _contexto.EmpleadoDeducciones
                .Include(e => e.Deduccion)
                .Where(e => e.IdEmpleado == idEmpleado && e.Deduccion!.Activa)
                .ToListAsync();
    }
}