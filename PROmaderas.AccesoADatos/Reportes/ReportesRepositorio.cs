using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Reportes
{
    public class ReportesRepositorio : IReportesRepositorio
    {
        private readonly Contexto _contexto;

        public ReportesRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }
        
        public async Task<List<FacturacionAD>> ObtenerFacturas()
        {
            return await _contexto.Facturaciones
                .Include(f => f.Cliente)
                .Where(f => f.Activa)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();
        }

        public async Task<List<ProductoAD>> ObtenerProductos()
        {
            return await _contexto.Productos
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<List<PlanillaDetalleFinancieroAD>> ObtenerPlanillaDetalles()
        {
            return await _contexto.PlanillaDetallesFinancieros
                .Include(d => d.Periodo)
                .Include(d => d.Empleado)
                .OrderByDescending(d => d.Periodo!.FechaInicio)
                .ToListAsync();
        }
    }
}