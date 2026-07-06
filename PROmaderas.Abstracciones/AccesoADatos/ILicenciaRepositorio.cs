using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface ILicenciaRepositorio
    {
        Task<List<LicenciaAD>> ObtenerTodas();
        Task<LicenciaAD?> ObtenerPorId(int id);
        Task<List<LicenciaAD>> ObtenerPorEmpleado(int idEmpleado);
        Task<LicenciaAD> Crear(LicenciaAD licencia);
        Task Actualizar(LicenciaAD licencia);
        Task Eliminar(int id);
    }
}