using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Puestos
{
    public class PuestoRepositorio : IPuestoRepositorio
    {
        private readonly Contexto _contexto;

        public PuestoRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        // dbo.Puesto no tiene columna "Activo"; la columna existente es "Estado".
        // Per spec, sin columna "Activo" literal devolvemos todos los puestos.
        public async Task<List<PuestoAD>> Listar()
        {
            return await _contexto.Puestos
                .OrderBy(p => p.NombrePuesto)
                .ToListAsync();
        }
    }
}
