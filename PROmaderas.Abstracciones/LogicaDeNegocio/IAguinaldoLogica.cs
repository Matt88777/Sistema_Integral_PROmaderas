using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    public interface IAguinaldoLogica
    {
        Task<List<AguinaldoResultadoAD>> CalcularPorAnio(int anio);
    }
}