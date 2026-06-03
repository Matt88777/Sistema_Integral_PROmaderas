using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IPuestoRepositorio
    {
        Task<List<PuestoAD>> Listar();
    }
}
