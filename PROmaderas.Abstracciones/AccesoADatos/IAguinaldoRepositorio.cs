using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IAguinaldoRepositorio
    {
        Task<List<PlanillaDetalleFinancieroAD>> ObtenerDetallesPorPeriodo(DateTime desde, DateTime hasta);
        Task<List<EmpleadoAD>> ObtenerEmpleados();
    }
}