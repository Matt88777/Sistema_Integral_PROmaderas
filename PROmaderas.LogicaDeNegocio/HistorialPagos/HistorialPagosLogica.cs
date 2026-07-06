using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.HistorialPagos
{
    public class HistorialPagosLogica : IHistorialPagosLogica
    {
        private readonly IHistorialPagosRepositorio _historialPagosRepositorio;

        public HistorialPagosLogica(IHistorialPagosRepositorio historialPagosRepositorio)
        {
            _historialPagosRepositorio = historialPagosRepositorio;
        }

        public async Task<List<PlanillaDetalleFinancieroAD>> ObtenerHistorialPorEmpleado(int idEmpleado)
        {
            return await _historialPagosRepositorio.ObtenerPorEmpleado(idEmpleado);
        }

        public async Task<List<EmpleadoAD>> ObtenerEmpleados()
        {
            return await _historialPagosRepositorio.ObtenerEmpleados();
        }
    }
}