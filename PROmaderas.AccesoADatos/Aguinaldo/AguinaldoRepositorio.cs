using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Aguinaldo
{
    public class AguinaldoRepositorio : IAguinaldoRepositorio
    {
        private readonly Contexto _contexto;

        public AguinaldoRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<List<PlanillaDetalleFinancieroAD>> ObtenerDetallesPorPeriodo(DateTime desde, DateTime hasta)
        {
            return await _contexto.PlanillaDetallesFinancieros
                .Include(d => d.Periodo)
                .Where(d => d.Periodo != null && d.Periodo.FechaInicio >= desde && d.Periodo.FechaInicio < hasta)
                .ToListAsync();
        }

        public async Task<List<EmpleadoAD>> ObtenerEmpleados()
        {
            return await _contexto.Empleados.ToListAsync();
        }
    }
}