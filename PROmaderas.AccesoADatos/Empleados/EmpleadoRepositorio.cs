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

            // PLA-HU-012: la FechaIngreso viene del formulario y se RESPETA. Antes se pisaba
            // siempre con DateTime.Now, así que todo empleado creado desde la app arrancaba con
            // 0 meses trabajados y por lo tanto 0 días de vacaciones acumulados, sin importar
            // qué antigüedad tuviera de verdad. DateTime.Now queda solo como último recurso
            // (formulario viejo que no manda el campo).
            empleado.FechaIngreso ??= DateTime.Now;

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

            // PLA-HU-012: la FechaIngreso que venga del formulario se RESPETA. Antes se pisaba
            // siempre con la existente, así que un empleado con la fecha nula no había forma de
            // corregirlo desde la app (y sin ella no se le pueden calcular vacaciones). Si el
            // formulario no la manda, se conserva la que ya estaba: editar el teléfono no puede
            // borrar la antigüedad de nadie.
            empleado.FechaIngreso ??= existente.FechaIngreso;

            empleado.FechaCreacion = existente.FechaCreacion;
            empleado.Estado = existente.Estado;
            empleado.PrimerApellido = existente.PrimerApellido;
            empleado.SegundoApellido = existente.SegundoApellido;

            // PLA-HU-012: SaldoVacacionesInicial ahora SÍ tiene campo en Empleados > Editar, así que
            // se respeta lo que venga del formulario (antes se restauraba a la fuerza desde la BD,
            // porque ningún form lo mandaba y el binder lo traía en 0, borrando el saldo migrado).
            //
            // OJO: es decimal NO-NULLABLE. Un POST que no incluya el campo bindea a 0, y 0 es un
            // valor legítimo, así que no hay forma de distinguir "no me lo mandaron" de "es cero".
            // Hoy no hay riesgo: Actualizar se llama SOLO desde EmpleadosController.Edit, cuyo
            // formulario ya trae el campo. Si mañana aparece otra pantalla o un endpoint que bindee
            // EmpleadoAD sin este campo, va a volver a borrar el dato en silencio.

            // Estos tres SÍ tienen combo en el formulario, pero si el combo ofrece un valor que no
            // existe en la BD, el <select> no puede marcarlo, cae en la opción vacía y el POST manda
            // "" -> el binder lo convierte en null y el Update lo guarda encima del dato bueno. Así
            // se borró 'Diurna' de JornadaLaboral. Los combos ya se corrigieron, pero el repositorio
            // no puede confiar en eso: un valor huérfano en la BD reproduce el problema. Vacío = "no
            // me mandaron nada", no "borrámelo".
            if (string.IsNullOrWhiteSpace(empleado.JornadaLaboral))
                empleado.JornadaLaboral = existente.JornadaLaboral;

            if (string.IsNullOrWhiteSpace(empleado.TipoPago))
                empleado.TipoPago = existente.TipoPago;

            if (string.IsNullOrWhiteSpace(empleado.Departamento))
                empleado.Departamento = existente.Departamento;

            // PLA-HU-002: registrar historial si el salario cambió.
            // Va DESPUÉS de las restauraciones de arriba a propósito: si se comparara contra los
            // nulls que venían del form, un simple cambio de teléfono se vería como un cambio de
            // salario y metería una fila espuria en SalarioHistorial.
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

            // FechaIngreso y SaldoVacacionesInicial entran a la bitácora: los dos son editables y los
            // dos mueven los días de vacaciones disponibles de todo el histórico del empleado (uno
            // por los meses trabajados, el otro sumando directo al saldo). Un cambio así no puede
            // quedar sin rastro.
            var valoresAnteriores = new
            {
                existente.IdPuesto,
                NombrePuesto = nombrePuestoAnterior,
                existente.Departamento,
                existente.FechaIngreso,
                existente.SaldoVacacionesInicial,
                existente.SalarioBase,
                existente.TipoPago,
                existente.JornadaLaboral
            };

            var valoresNuevos = new
            {
                empleado.IdPuesto,
                NombrePuesto = nombrePuestoNuevo,
                empleado.Departamento,
                empleado.FechaIngreso,
                empleado.SaldoVacacionesInicial,
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