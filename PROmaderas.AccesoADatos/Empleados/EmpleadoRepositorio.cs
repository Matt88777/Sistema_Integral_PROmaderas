using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos.Auditoria;

namespace PROmaderas.AccesoADatos.Empleados
{
    public class EmpleadoRepositorio : IEmpleadoRepositorio
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
                .Where(e => e.Nombre.Contains(filtro) || e.Cedula.Contains(filtro))
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

        public async Task Actualizar(EmpleadoAD empleado, ContextoAuditoria auditoria)
        {
            var existente = await _contexto.Empleados
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEmpleado == empleado.IdEmpleado);

            if (existente == null)
                throw new Exception($"No se encontró el empleado con ID {empleado.IdEmpleado}.");

            empleado.FechaIngreso = existente.FechaIngreso;
            empleado.FechaCreacion = existente.FechaCreacion;
            empleado.Estado = existente.Estado;
            empleado.PrimerApellido = existente.PrimerApellido;
            empleado.SegundoApellido = existente.SegundoApellido;

            // PLA-HU-002: registrar historial si el salario cambió
            bool salarioCambio =
                existente.SalarioBase != empleado.SalarioBase ||
                existente.TipoPago != empleado.TipoPago ||
                existente.JornadaLaboral != empleado.JornadaLaboral;

            if (salarioCambio)
            {
                _contexto.SalarioHistoriales.Add(new SalarioHistorialAD
                {
                    IdEmpleado = empleado.IdEmpleado ?? 0,
                    SalarioBase = empleado.SalarioBase,
                    TipoPago = empleado.TipoPago,
                    JornadaLaboral = empleado.JornadaLaboral,
                    FechaCambio = DateTime.Now,
                    UsuarioResponsable = auditoria.Email
                });
            }

            var nombrePuestoAnterior = await _contexto.Puestos.AsNoTracking()
                .Where(p => p.IdPuesto == existente.IdPuesto)
                .Select(p => p.NombrePuesto)
                .FirstOrDefaultAsync();
            var nombrePuestoNuevo = await _contexto.Puestos.AsNoTracking()
                .Where(p => p.IdPuesto == empleado.IdPuesto)
                .Select(p => p.NombrePuesto)
                .FirstOrDefaultAsync();

            var valoresAnteriores = new
            {
                existente.IdPuesto,
                NombrePuesto = nombrePuestoAnterior,
                existente.Departamento,
                existente.SalarioBase,
                existente.TipoPago,
                existente.JornadaLaboral
            };

            var valoresNuevos = new
            {
                empleado.IdPuesto,
                NombrePuesto = nombrePuestoNuevo,
                empleado.Departamento,
                empleado.SalarioBase,
                empleado.TipoPago,
                empleado.JornadaLaboral
            };

            _contexto.Empleados.Update(empleado);
            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                "Empleado",
                empleado.IdEmpleado,
                auditoria,
                valoresAnteriores,
                valoresNuevos));

            await _contexto.SaveChangesAsync();
        }

        public async Task CambiarEstado(int id, ContextoAuditoria auditoria)
        {
            var empleado = await _contexto.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == id);

            if (empleado == null)
                throw new Exception($"No se encontró el empleado con ID {id}.");

            var estadoAnterior = empleado.Estado ?? true;
            var estadoNuevo = !estadoAnterior;
            empleado.Estado = estadoNuevo;

            _contexto.Empleados.Update(empleado);
            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                "Empleado",
                empleado.IdEmpleado,
                auditoria,
                new { Estado = estadoAnterior ? "Activo" : "Inactivo" },
                new { Estado = estadoNuevo ? "Activo" : "Inactivo" }));

            await _contexto.SaveChangesAsync();
        }

        public async Task Eliminar(int id)
        {
            var empleado = await _contexto.Empleados.FindAsync(id);
            if (empleado != null)
            {
                _contexto.Empleados.Remove(empleado);
                await _contexto.SaveChangesAsync();
            }
        }

        public async Task<List<SalarioHistorialAD>> ObtenerHistorialSalario(int idEmpleado)
        {
            return await _contexto.SalarioHistoriales
                .Where(h => h.IdEmpleado == idEmpleado)
                .OrderByDescending(h => h.FechaCambio)
                .ToListAsync();
        }
    }
}