using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Produccion
{
    /// <summary>
    /// Acceso a datos para la producción semanal de tarimas.
    /// INV-HU-001: inserta en InventarioMovimiento (tipo "Entrada") y
    /// el stock se recalcula dinámicamente en ProductoRepositorio.
    /// </summary>
    public class ProduccionRepositorio : IProduccionRepositorio
    {
        private readonly Contexto _contexto;

        public ProduccionRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        /// <inheritdoc/>
        public async Task<InventarioMovimientoAD> RegistrarProduccion(InventarioMovimientoAD movimiento)
        {
            _contexto.InventarioMovimientos.Add(movimiento);
            await _contexto.SaveChangesAsync();
            return movimiento;
        }

        /// <inheritdoc/>
        public async Task<List<InventarioMovimientoAD>> ObtenerHistorialProduccion(
            int pagina, int registrosPorPagina)
        {
            return await _contexto.InventarioMovimientos
                .Where(m => m.TipoMovimiento == TiposMovimientoInventario.Entrada)
                .OrderByDescending(m => m.FechaMovimiento)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Include(m => m.Producto)   // para mostrar nombre del tipo de tarima
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<int> ContarMovimientosProduccion()
        {
            return await _contexto.InventarioMovimientos
                .CountAsync(m => m.TipoMovimiento == TiposMovimientoInventario.Entrada);
        }

        /// <inheritdoc/>
        public async Task<bool> TipoTarimaExiste(int idTipoTarima)
        {
            return await _contexto.Productos
                .AnyAsync(p => p.Id == idTipoTarima && p.Activo);
        }

        /// <inheritdoc/>
        public async Task<int?> ObtenerIdUsuarioPorCorreo(string correo)
        {
            var correoNorm = (correo ?? string.Empty).Trim().ToLower();
            var usuario = await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == correoNorm);
            return usuario?.IdUsuario;
        }
    }
}