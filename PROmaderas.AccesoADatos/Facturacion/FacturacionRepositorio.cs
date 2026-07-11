using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos.Auditoria;

namespace PROmaderas.AccesoADatos.Facturacion
{
    public class FacturacionRepositorio : IFacturacionRepositorio
    {
        private readonly Contexto _contexto;

        public FacturacionRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<List<FacturacionAD>> ObtenerTodas(bool incluirInactivas = false)
        {
            var consulta = _contexto.Facturaciones
                .Include(f => f.Pedido)
                .Include(f => f.Cliente)
                .AsQueryable();

            // FAC-HU-005: el filtro por Activa solo se aplica cuando NO se piden las inactivas.
            if (!incluirInactivas)
                consulta = consulta.Where(f => f.Activa);

            return await consulta
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();
        }

        public async Task<List<FacturacionAD>> BuscarConFiltros(int? clienteId, DateTime? fechaDesde,
                                                                DateTime? fechaHasta, string? numeroFactura,
                                                                bool incluirInactivas = false)
        {
            // Mismo query base que ObtenerTodas; se le suman .Where() condicionales.
            var consulta = _contexto.Facturaciones
                .Include(f => f.Pedido)
                .Include(f => f.Cliente)
                .AsQueryable();

            // FAC-HU-005: sin esto, una factura inactivada desaparecería del Index
            // y no habría forma de encontrarla para reactivarla.
            if (!incluirInactivas)
                consulta = consulta.Where(f => f.Activa);

            if (clienteId.HasValue)
                consulta = consulta.Where(f => f.ClienteId == clienteId.Value);

            if (fechaDesde.HasValue)
                consulta = consulta.Where(f => f.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                // Sumar un día para incluir todo el día "hasta", ya que Fecha tiene hora.
                consulta = consulta.Where(f => f.Fecha < fechaHasta.Value.AddDays(1));

            if (!string.IsNullOrWhiteSpace(numeroFactura))
                consulta = consulta.Where(f => f.NumeroFactura.Contains(numeroFactura));

            return await consulta
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

        public async Task CambiarEstado(int id, string nuevoEstado, ContextoAuditoria auditoria)
        {
            // Mismo patrón atómico que ClienteRepositorio.CambiarEstado:
            // Update + Bitacoras.Add en UN solo SaveChangesAsync.
            var factura = await _contexto.Facturaciones
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factura == null)
                throw new Exception($"No se encontró la factura con ID {id}.");

            var estadoAnterior = factura.Estado;

            var valoresAnteriores = new { Estado = estadoAnterior };
            var valoresNuevos = new { Estado = nuevoEstado };

            // Regla "sin alterar el documento original": solo se toca Estado.
            factura.Estado = nuevoEstado;

            _contexto.Facturaciones.Update(factura);

            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                "Factura",
                factura.Id,
                auditoria,
                valoresAnteriores,
                valoresNuevos));

            await _contexto.SaveChangesAsync();
        }

        public async Task CambiarActiva(int id, bool activa, string nuevoEstado, string motivo,
                                        ContextoAuditoria auditoria)
        {
            // Mismo patrón atómico que CambiarEstado: UPDATE + INSERT bitácora en UN SaveChangesAsync.
            var factura = await _contexto.Facturaciones
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factura == null)
                throw new Exception($"No se encontró la factura con ID {id}.");

            // Se capturan ANTES de mutar: el Estado previo es la evidencia que exige la HU.
            var valoresAnteriores = new { factura.Activa, factura.Estado };

            factura.Activa = activa;
            factura.Estado = nuevoEstado;

            _contexto.Facturaciones.Update(factura);

            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(
                "Factura",
                factura.Id,
                auditoria,
                valoresAnteriores,
                new { Activa = activa, Estado = nuevoEstado, Motivo = motivo }));

            await _contexto.SaveChangesAsync();
        }

        public async Task<BitacoraAuditoriaAD?> ObtenerUltimaBitacoraFactura(int idFactura, string accion)
        {
            return await _contexto.Bitacoras
                .Where(b => b.TablaAfectada == "Factura"
                         && b.IdRegistroAfectado == idFactura
                         && b.Accion == accion)
                .OrderByDescending(b => b.FechaAccion)
                .ThenByDescending(b => b.IdBitacora)   // desempate si dos quedan en el mismo tick
                .FirstOrDefaultAsync();
        }

        public async Task<FacturacionAD?> ObtenerOtraFacturaActivaDeOrden(int pedidoId, int idFacturaExcluida)
        {
            // Include(Pedido) para poder nombrar la orden en el mensaje de error de la Lógica.
            return await _contexto.Facturaciones
                .Include(f => f.Pedido)
                .FirstOrDefaultAsync(f => f.PedidoId == pedidoId
                                       && f.Activa
                                       && f.Id != idFacturaExcluida);
        }

        public async Task RegistrarPago(PagoFacturaAD pago, ContextoAuditoria auditoria)
        {
            // Patrón atómico extendido de CambiarEstado: 3 operaciones en UN SaveChangesAsync.
            var factura = await _contexto.Facturaciones
                .FirstOrDefaultAsync(f => f.Id == pago.IdFactura);

            if (factura == null)
                throw new Exception($"No se encontró la factura con ID {pago.IdFactura}.");

            var saldoAnterior = factura.SaldoPendiente;
            var estadoAnterior = factura.Estado;

            // Decimal exacto (sin pérdida de precisión): saldo y monto son decimal(18,2).
            factura.SaldoPendiente = factura.SaldoPendiente - pago.Monto;
            factura.Estado = factura.SaldoPendiente == 0
                ? EstadosFactura.Pagada
                : EstadosFactura.PendienteDePago;

            _contexto.PagosFactura.Add(pago);                 // INSERT pago
            _contexto.Facturaciones.Update(factura);          // UPDATE saldo/estado
            _contexto.Bitacoras.Add(ConstructorBitacora.Construir(  // INSERT bitácora
                "Factura",
                factura.Id,
                auditoria,
                new { SaldoPendiente = saldoAnterior, Estado = estadoAnterior },
                new
                {
                    Pago = new { pago.Monto, pago.FormaPago, pago.Referencia, pago.FechaPago },
                    SaldoPendiente = factura.SaldoPendiente,
                    Estado = factura.Estado
                }));

            await _contexto.SaveChangesAsync();               // los 3 en una sola transacción
        }

        public async Task<List<PagoFacturaAD>> ObtenerPagosPorFactura(int idFactura)
        {
            return await _contexto.PagosFactura
                .Where(p => p.IdFactura == idFactura)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
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
