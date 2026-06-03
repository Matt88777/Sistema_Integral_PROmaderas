using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Puestos
{
    public class PuestoLogica : IPuestoLogica
    {
        private readonly IPuestoRepositorio _repositorio;

        public PuestoLogica(IPuestoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<PuestoAD>> Listar()
        {
            return await _repositorio.Listar();
        }
    }
}
