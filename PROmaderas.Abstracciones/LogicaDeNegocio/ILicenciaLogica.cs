using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    public interface ILicenciaLogica
    {
        Task<List<LicenciaAD>> ObtenerTodas();
        Task<LicenciaAD?> ObtenerPorId(int id);
        Task<List<LicenciaAD>> ObtenerPorEmpleado(int idEmpleado);
        Task<LicenciaAD> Registrar(LicenciaAD licencia);
        Task Actualizar(LicenciaAD licencia);
        Task Eliminar(int id);
    }
}