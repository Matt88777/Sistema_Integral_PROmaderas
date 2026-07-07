using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Deducciones
{
    public class DeduccionInternaRepositorio : IDeduccionInternaRepositorio
    {
        private readonly Contexto _contexto;

        public DeduccionInternaRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<List<DeduccionInternaAD>> ObtenerTodas()
            => await _contexto.DeduccionesInternas.OrderBy(d => d.Nombre).ToListAsync();

        public async Task<DeduccionInternaAD?> ObtenerPorId(int id)
            => await _contexto.DeduccionesInternas.FindAsync(id);

        public async Task<DeduccionInternaAD> Crear(DeduccionInternaAD deduccion)
        {
            _contexto.DeduccionesInternas.Add(deduccion);
            await _contexto.SaveChangesAsync();
            return deduccion;
        }

        public async Task Actualizar(DeduccionInternaAD deduccion)
        {
            _contexto.DeduccionesInternas.Update(deduccion);
            await _contexto.SaveChangesAsync();
        }

        public async Task Eliminar(int id)
        {
            var d = await _contexto.DeduccionesInternas.FindAsync(id);
            if (d != null)
            {
                _contexto.DeduccionesInternas.Remove(d);
                await _contexto.SaveChangesAsync();
            }
        }

        public async Task<List<EmpleadoDeduccionAD>> ObtenerAsignacionesPorEmpleado(int idEmpleado)
            => await _contexto.EmpleadoDeducciones
                .Include(e => e.Deduccion)
                .Include(e => e.Empleado)
                .Where(e => e.IdEmpleado == idEmpleado)
                .ToListAsync();

        public async Task AsignarAEmpleado(int idEmpleado, int idDeduccion)
        {
            var yaExiste = await _contexto.EmpleadoDeducciones
                .AnyAsync(e => e.IdEmpleado == idEmpleado && e.IdDeduccion == idDeduccion);

            if (!yaExiste)
            {
                _contexto.EmpleadoDeducciones.Add(new EmpleadoDeduccionAD
                {
                    IdEmpleado = idEmpleado,
                    IdDeduccion = idDeduccion
                });
                await _contexto.SaveChangesAsync();
            }
        }

        public async Task DesasignarDeEmpleado(int idEmpleadoDeduccion)
        {
            var asig = await _contexto.EmpleadoDeducciones.FindAsync(idEmpleadoDeduccion);
            if (asig != null)
            {
                _contexto.EmpleadoDeducciones.Remove(asig);
                await _contexto.SaveChangesAsync();
            }
        }

        public async Task<List<EmpleadoDeduccionAD>> ObtenerDeduccionesActivasDeEmpleado(int idEmpleado)
            => await _contexto.EmpleadoDeducciones
                .Include(e => e.Deduccion)
                .Where(e => e.IdEmpleado == idEmpleado && e.Deduccion!.Activa)
                .ToListAsync();
    }
}