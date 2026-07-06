using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Licencias
{
    public class LicenciaRepositorio : ILicenciaRepositorio
    {
        private readonly Contexto _contexto;

        public LicenciaRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<List<LicenciaAD>> ObtenerTodas()
        {
            return await _contexto.Licencias
                .Include(l => l.Empleado)
                .OrderByDescending(l => l.FechaInicio)
                .ToListAsync();
        }

        public async Task<LicenciaAD?> ObtenerPorId(int id)
        {
            return await _contexto.Licencias
                .Include(l => l.Empleado)
                .FirstOrDefaultAsync(l => l.IdLicencia == id);
        }

        public async Task<List<LicenciaAD>> ObtenerPorEmpleado(int idEmpleado)
        {
            return await _contexto.Licencias
                .Where(l => l.IdEmpleado == idEmpleado)
                .OrderByDescending(l => l.FechaInicio)
                .ToListAsync();
        }

        public async Task<LicenciaAD> Crear(LicenciaAD licencia)
        {
            _contexto.Licencias.Add(licencia);
            await _contexto.SaveChangesAsync();
            return licencia;
        }

        public async Task Actualizar(LicenciaAD licencia)
        {
            _contexto.Licencias.Update(licencia);
            await _contexto.SaveChangesAsync();
        }

        public async Task Eliminar(int id)
        {
            var licencia = await _contexto.Licencias.FindAsync(id);
            if (licencia != null)
            {
                _contexto.Licencias.Remove(licencia);
                await _contexto.SaveChangesAsync();
            }
        }
    }
}