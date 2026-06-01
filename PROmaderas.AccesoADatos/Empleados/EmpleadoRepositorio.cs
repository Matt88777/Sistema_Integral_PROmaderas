using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Empleados
{
    public class EmpleadoRepositorio
    {
        private readonly Contexto _contexto;

        public EmpleadoRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<List<EmpleadoAD>> ObtenerTodos()
        {
            return await _contexto.Empleados.ToListAsync();
        }

        public async Task<List<EmpleadoAD>> Buscar(string filtro)
        {
            return await _contexto.Empleados
                .Where(e =>
                    e.Nombre.Contains(filtro) ||
                    e.Cedula.Contains(filtro))
                .ToListAsync();
        }

        public async Task<EmpleadoAD> Crear(EmpleadoAD empleado)
        {
            empleado.PrimerApellido = empleado.PrimerApellido ?? "General";
            empleado.SegundoApellido = empleado.SegundoApellido ?? "General";
            empleado.FechaIngreso = DateTime.Now;
            empleado.IdPuesto = 1;
            empleado.Estado = true;
            empleado.FechaCreacion = DateTime.Now;

            _contexto.Empleados.Add(empleado);
            await _contexto.SaveChangesAsync();

            return empleado;
        }

        //Metodo actualizar nuevo
        public async Task Actualizar(EmpleadoAD empleado)
        {
            var existente = await _contexto.Empleados
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEmpleado == empleado.IdEmpleado);

            if (existente == null)
                throw new Exception($"No se encontró el empleado con ID {empleado.IdEmpleado}.");

            // Conservar los campos que no se editan en el formulario
            empleado.FechaIngreso = existente.FechaIngreso;
            empleado.FechaCreacion = existente.FechaCreacion;
            empleado.IdPuesto = existente.IdPuesto;
            empleado.Estado = existente.Estado;
            empleado.PrimerApellido = existente.PrimerApellido;
            empleado.SegundoApellido = existente.SegundoApellido;

            _contexto.Empleados.Update(empleado);
            await _contexto.SaveChangesAsync();
        }
        //Metodo actualizar antiguo
        //public async Task Actualizar(EmpleadoAD empleado)
        //{
        //    var existente = await _contexto.Empleados.FindAsync(empleado.IdEmpleado);

        //    if (existente != null)
        //    {
        //        existente.Nombre = empleado.Nombre;
        //        existente.Cedula = empleado.Cedula;
        //        existente.Departamento = empleado.Departamento;
        //        existente.Telefono = empleado.Telefono;
        //        existente.Correo = empleado.Correo;
        //        existente.IdPuesto = empleado.IdPuesto;
        //
        //        await _contexto.SaveChangesAsync();
        //    }
        //}

        public async Task Eliminar(int id)
        {
            var empleado = await _contexto.Empleados.FindAsync(id);
            if (empleado != null)
            {
                _contexto.Empleados.Remove(empleado);
                await _contexto.SaveChangesAsync();
            }
        }
    }
}