using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Facturacion
{
    public class FacturacionRepositorio : IFacturacionRepositorio
    {
        private readonly Contexto _contexto;

        public FacturacionRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<List<FacturacionAD>> ObtenerTodasActivas()
        {
            return await _contexto.Facturaciones
                .Include(f => f.Pedido)
                .Include(f => f.Cliente)
                .Where(f => f.Activa)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();
        }

        public async Task<FacturacionAD?> ObtenerPorId(int id)
        {
            return await _contexto.Facturaciones
                .Include(f => f.Pedido)
                    .ThenInclude(p => p!.Detalles!)
                        .ThenInclude(d => d.Producto)
                .Include(f => f.Cliente)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<FacturacionAD> Crear(FacturacionAD factura)
        {
            _contexto.Facturaciones.Add(factura);
            await _contexto.SaveChangesAsync();
            return factura;
        }

        public async Task<int> ObtenerMaximoId()
        {
            return await _contexto.Facturaciones.MaxAsync(f => (int?)f.Id) ?? 0;
        }

        public async Task<bool> PedidoYaFacturado(int pedidoId)
        {
            return await _contexto.Facturaciones
                .AnyAsync(f => f.PedidoId == pedidoId && f.Activa);
        }

        public async Task<List<PedidoAD>> ObtenerOrdenesFacturables()
        {
            var idsYaFacturados = await _contexto.Facturaciones
                .Where(f => f.Activa).Select(f => f.PedidoId).ToListAsync();

            return await _contexto.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.Activa && p.Estado != "Cancelada"
                         && !idsYaFacturados.Contains(p.Id))
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
        }

        public async Task<PedidoAD?> ObtenerPedidoParaFacturar(int pedidoId)
        {
            return await _contexto.Pedidos
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);
        }

        public async Task<int?> ObtenerIdUsuarioPorCorreo(string correo)
        {
            var correoNorm = (correo ?? string.Empty).ToLower();
            var usuario = await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == correoNorm);
            return usuario?.IdUsuario;
        }
    }
}
