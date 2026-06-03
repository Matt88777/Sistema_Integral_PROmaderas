using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    public interface IPuestoLogica
    {
        Task<List<PuestoAD>> Listar();
    }
}
