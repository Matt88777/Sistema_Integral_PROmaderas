using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Categorias
{
    public class CategoriaRepositorio : ICategoriaRepositorio
    {
        private readonly Contexto _contexto;

        public CategoriaRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public Task<List<CategoriaAD>> ObtenerTodas()
        {
            return _contexto.Categorias
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public Task<CategoriaAD?> ObtenerPorId(int id)
        {
            return _contexto.Categorias
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CategoriaAD> Crear(CategoriaAD categoria)
        {
            _contexto.Categorias.Add(categoria);
            await _contexto.SaveChangesAsync();
            return categoria;
        }

        public async Task<CategoriaAD> Actualizar(CategoriaAD categoria)
        {
            _contexto.Categorias.Update(categoria);
            await _contexto.SaveChangesAsync();
            return categoria;
        }

        public async Task<bool> Eliminar(int id)
        {
            var categoria = await ObtenerPorId(id);
            if (categoria == null) return false;

            categoria.Activo = false;
            await Actualizar(categoria);
            return true;
        }

        public Task<bool> Existe(int id)
        {
            return _contexto.Categorias.AnyAsync(c => c.Id == id);
        }
    }
}