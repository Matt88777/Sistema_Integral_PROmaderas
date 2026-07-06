using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Licencias
{
    public class LicenciaLogica : ILicenciaLogica
    {
        private readonly ILicenciaRepositorio _licenciaRepositorio;

        public LicenciaLogica(ILicenciaRepositorio licenciaRepositorio)
        {
            _licenciaRepositorio = licenciaRepositorio;
        }

        public async Task<List<LicenciaAD>> ObtenerTodas()
        {
            return await _licenciaRepositorio.ObtenerTodas();
        }

        public async Task<LicenciaAD?> ObtenerPorId(int id)
        {
            return await _licenciaRepositorio.ObtenerPorId(id);
        }

        public async Task<List<LicenciaAD>> ObtenerPorEmpleado(int idEmpleado)
        {
            return await _licenciaRepositorio.ObtenerPorEmpleado(idEmpleado);
        }

        public async Task<LicenciaAD> Registrar(LicenciaAD licencia)
        {
            if (licencia.FechaFin < licencia.FechaInicio)
                throw new InvalidOperationException("La fecha fin no puede ser menor a la fecha de inicio.");

            licencia.Dias = (decimal)(licencia.FechaFin.Date - licencia.FechaInicio.Date).TotalDays + 1;

            return await _licenciaRepositorio.Crear(licencia);
        }

        public async Task Actualizar(LicenciaAD licencia)
        {
            if (licencia.FechaFin < licencia.FechaInicio)
                throw new InvalidOperationException("La fecha fin no puede ser menor a la fecha de inicio.");

            licencia.Dias = (decimal)(licencia.FechaFin.Date - licencia.FechaInicio.Date).TotalDays + 1;

            await _licenciaRepositorio.Actualizar(licencia);
        }

        public async Task Eliminar(int id)
        {
            await _licenciaRepositorio.Eliminar(id);
        }
    }
}