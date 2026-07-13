using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos.Auditoria;

namespace PROmaderas.AccesoADatos.Liquidaciones
{
    public class LiquidacionRepositorio : ILiquidacionRepositorio
    {
        private const string Tabla = "Liquidacion";

        private readonly Contexto _contexto;

        public LiquidacionRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        // ── Consultas ─────────────────────────────────────────────────────────

        // Trae TODAS, incluidas las anuladas: el listado las muestra atenuadas.
        public async Task<List<LiquidacionAD>> ObtenerTodas()
            => await _contexto.Liquidaciones
                .OrderByDescending(l => l.FechaCalculo)
                .ThenByDescending(l => l.IdLiquidacion)
                .ToListAsync();

        public async Task<LiquidacionAD?> ObtenerPorId(int id)
            => await _contexto.Liquidaciones
                .FirstOrDefaultAsync(l => l.IdLiquidacion == id);

        public async Task<List<LiquidacionAD>> ObtenerPorEmpleado(int idEmpleado)
            => await _contexto.Liquidaciones
                .Where(l => l.IdEmpleado == idEmpleado)
                .OrderByDescending(l => l.FechaCalculo)
                .ToListAsync();

        // Activa = la liquidación sigue en pie (no se anuló). Si existe, esa persona ya fue
        // liquidada y no se puede volver a liquidar.
        public async Task<bool> TieneLiquidacionActiva(int idEmpleado)
            => await _contexto.Liquidaciones
                .AnyAsync(l => l.IdEmpleado == idEmpleado && l.Activa);

        public async Task<List<EmpleadoAD>> ObtenerEmpleadosActivos()
            => await _contexto.Empleados
                .Where(e => e.Estado == true)
                .OrderBy(e => e.Nombre)
                .ToListAsync();

        public async Task<EmpleadoAD?> ObtenerEmpleadoPorId(int idEmpleado)
            => await _contexto.Empleados
                .FirstOrDefaultAsync(e => e.IdEmpleado == idEmpleado);

        public async Task<Dictionary<int, string>> ObtenerNombresEmpleados()
        {
            // TODOS, sin filtrar por Estado: un empleado liquidado está inactivo, y es
            // precisamente el que aparece en el listado de liquidaciones. UN solo query.
            var empleados = await _contexto.Empleados
                .Select(e => new
                {
                    e.IdEmpleado,
                    e.Nombre,
                    e.PrimerApellido,
                    e.SegundoApellido
                })
                .ToListAsync();

            return empleados
                .Where(e => e.IdEmpleado.HasValue)
                .ToDictionary(
                    e => e.IdEmpleado!.Value,
                    e => string.Join(" ", new[] { e.Nombre, e.PrimerApellido, e.SegundoApellido }
                                            .Where(p => !string.IsNullOrWhiteSpace(p))));
        }

        // Mismo puente que FacturacionRepositorio.ObtenerIdUsuarioPorCorreo: la FK apunta a
        // dbo.Usuario (int) pero Identity maneja GUIDs, y el único dato en común es el correo.
        // Devuelve null si no lo encuentra; el fallback a admin lo decide la Lógica.
        public async Task<int?> ObtenerIdUsuarioPorCorreo(string? correo)
        {
            var correoNorm = (correo ?? string.Empty).ToLower();
            var usuario = await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == correoNorm);
            return usuario?.IdUsuario;
        }

        // ── Escrituras ────────────────────────────────────────────────────────

        public async Task Guardar(LiquidacionAD liquidacion, ContextoAuditoria auditoria)
        {
            // El empleado se carga TRACKED (sin AsNoTracking) y se mutan SOLO los 3 campos del
            // cierre. Nada de Update() sobre una entidad detached: eso reescribe la fila completa
            // y borra en silencio todo lo que no venga cargado.
            var empleado = await _contexto.Empleados
                .FirstOrDefaultAsync(e => e.IdEmpleado == liquidacion.IdEmpleado);

            if (empleado == null)
                throw new Exception($"No se encontró el empleado con ID {liquidacion.IdEmpleado}.");

            var valoresAnteriores = new
            {
                empleado.IdEmpleado,
                empleado.Estado,
                empleado.FechaSalida,
                empleado.MotivoSalida
            };

            _contexto.Liquidaciones.Add(liquidacion);

            // El cierre de la relación laboral. Estado = false directo, NO por CambiarEstado:
            // ese método es un TOGGLE (!estadoAnterior) y llamarlo sobre alguien ya inactivo lo
            // REACTIVARÍA. Acá el resultado tiene que ser "inactivo", no "el contrario de lo que
            // estaba".
            empleado.Estado = false;
            empleado.FechaSalida = liquidacion.FechaSalida;
            empleado.MotivoSalida = liquidacion.MotivoSalida;

            // IdRegistroAfectado = null: IdLiquidacion es IDENTITY, no existe hasta el SaveChanges.
            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                Tabla,
                null,
                auditoria,
                valoresAnteriores,
                new
                {
                    liquidacion.IdEmpleado,
                    liquidacion.FechaIngreso,
                    liquidacion.FechaSalida,
                    liquidacion.MotivoSalida,
                    liquidacion.AniosLaborados,
                    liquidacion.SalarioPromedio,
                    liquidacion.DiasVacacionesPendientes,
                    liquidacion.MontoVacaciones,
                    liquidacion.MontoAguinaldoProporcional,
                    liquidacion.MontoPreaviso,
                    liquidacion.MontoCesantia,
                    liquidacion.OtrosMontos,
                    liquidacion.TotalLiquidacion,
                    liquidacion.Estado,
                    liquidacion.Observacion,
                    EmpleadoInactivado = true
                }));

            // INSERT de la liquidación + cierre del empleado + bitácora: una sola transacción.
            // Si algo falla, no queda un empleado despedido sin su liquidación.
            await _contexto.SaveChangesAsync();
        }

        public async Task Anular(int idLiquidacion, string motivo, ContextoAuditoria auditoria)
        {
            var liquidacion = await _contexto.Liquidaciones
                .FirstOrDefaultAsync(l => l.IdLiquidacion == idLiquidacion);

            if (liquidacion == null)
                throw new Exception($"No se encontró la liquidación con ID {idLiquidacion}.");

            // Se capturan ANTES de mutar: el total y el estado previos son la evidencia.
            var valoresAnteriores = new
            {
                liquidacion.IdEmpleado,
                liquidacion.FechaSalida,
                liquidacion.MotivoSalida,
                liquidacion.TotalLiquidacion,
                liquidacion.Estado,
                liquidacion.Activa
            };

            // Soft-delete: la fila queda como evidencia de que se emitió y se anuló.
            liquidacion.Activa = false;
            liquidacion.Estado = EstadosLiquidacion.Anulada;

            _contexto.Liquidaciones.Update(liquidacion);

            // Si la liquidación se anuló, el despido NO ocurrió: el empleado vuelve a estar
            // activo y sin datos de salida. Dejarlo inactivo con FechaSalida puesta lo sacaría
            // de planilla y de vacaciones para siempre, sin ninguna liquidación que lo respalde.
            var empleado = await _contexto.Empleados
                .FirstOrDefaultAsync(e => e.IdEmpleado == liquidacion.IdEmpleado);

            object? reapertura = null;

            if (empleado != null)
            {
                reapertura = new
                {
                    empleado.IdEmpleado,
                    EstadoPrevio = empleado.Estado,
                    FechaSalidaPrevia = empleado.FechaSalida,
                    MotivoSalidaPrevio = empleado.MotivoSalida
                };

                empleado.Estado = true;
                empleado.FechaSalida = null;
                empleado.MotivoSalida = null;
            }

            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                Tabla,
                liquidacion.IdLiquidacion,
                auditoria,
                valoresAnteriores,
                new
                {
                    liquidacion.IdEmpleado,
                    liquidacion.TotalLiquidacion,
                    Estado = EstadosLiquidacion.Anulada,
                    Activa = false,
                    Motivo = motivo,
                    EmpleadoReactivado = reapertura   // null si el empleado ya no existe
                }));

            // Anulación + reapertura del empleado + bitácora: una sola transacción.
            await _contexto.SaveChangesAsync();
        }
    }
}
