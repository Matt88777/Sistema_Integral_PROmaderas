using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.HistorialPagos
{
    public class HistorialPagosRepositorio : IHistorialPagosRepositorio
    {
        private readonly Contexto _contexto;

        public HistorialPagosRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<List<PlanillaDetalleFinancieroAD>> ObtenerPorEmpleado(int idEmpleado)
        {
            return await _contexto.PlanillaDetallesFinancieros
                .Include(d => d.Periodo)
                .Where(d => d.IdEmpleado == idEmpleado)
                .OrderByDescending(d => d.Periodo!.FechaInicio)
                .ToListAsync();
        }

        public async Task<List<EmpleadoAD>> ObtenerEmpleados()
        {
            return await _contexto.Empleados
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }
    }
}