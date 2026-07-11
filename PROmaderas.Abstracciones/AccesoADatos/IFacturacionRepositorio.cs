using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IFacturacionRepositorio
    {
        // Listado del Index: facturas con Pedido y Cliente, más recientes primero.
        // incluirInactivas = false (default) aplica el filtro .Where(f => f.Activa).
        Task<List<FacturacionAD>> ObtenerTodas(bool incluirInactivas = false);

        // Listado del Index con filtros opcionales (cliente, rango de fechas, número consecutivo).
        // FAC-HU-005: incluirInactivas permite que el Administrador vea las facturas anuladas.
        Task<List<FacturacionAD>> BuscarConFiltros(int? clienteId, DateTime? fechaDesde,
                                                   DateTime? fechaHasta, string? numeroFactura,
                                                   bool incluirInactivas = false);

        // Detalle: factura con Pedido -> Detalles -> Producto y Cliente.
        // Sin filtro por Activa: el Details debe poder abrir facturas anuladas.
        Task<FacturacionAD?> ObtenerPorId(int id);

        // Inserta la factura y guarda.
        Task<FacturacionAD> Crear(FacturacionAD factura);

        // Cambia SOLO el campo Estado y registra la bitácora en el mismo SaveChanges (atómico).
        Task CambiarEstado(int id, string nuevoEstado, ContextoAuditoria auditoria);

        // FAC-HU-005: UPDATE Activa + Estado + INSERT bitácora (con el motivo) en UN SaveChanges.
        // El Estado anterior queda en ValorAnterior de la bitácora: esa es la evidencia.
        Task CambiarActiva(int id, bool activa, string nuevoEstado, string motivo, ContextoAuditoria auditoria);

        // FAC-HU-005: última entrada de bitácora de una factura para cierta acción
        // (el Details lee de ahí el motivo de la anulación). Null si no hay.
        Task<BitacoraAuditoriaAD?> ObtenerUltimaBitacoraFactura(int idFactura, string accion);

        // FAC-HU-005: otra factura ACTIVA (Id distinto) de la misma orden, con su Pedido cargado.
        // Al anular una factura la orden vuelve a ser facturable, así que mientras estuvo anulada
        // pudieron emitirle otra: sin este chequeo, reactivarla dejaría DOS activas para la orden.
        Task<FacturacionAD?> ObtenerOtraFacturaActivaDeOrden(int pedidoId, int idFacturaExcluida);

        // FAC-HU-004: INSERT pago + UPDATE factura (saldo/estado) + INSERT bitácora en UN SaveChanges.
        Task RegistrarPago(PagoFacturaAD pago, ContextoAuditoria auditoria);

        // Historial de pagos de una factura (para Details).
        Task<List<PagoFacturaAD>> ObtenerPagosPorFactura(int idFactura);

        // Mayor Id existente (0 si no hay) para calcular el consecutivo.
        Task<int> ObtenerMaximoId();

        // ¿La orden ya tiene una factura activa?
        // Ojo: acá el filtro por Activa es REGLA DE NEGOCIO (una orden anulada se puede volver
        // a facturar), no un filtro de listado. No lleva incluirInactivas.
        Task<bool> PedidoYaFacturado(int pedidoId);

        // Órdenes que se pueden facturar: activas, no canceladas y aún sin factura activa.
        // Mismo criterio que PedidoYaFacturado: el filtro por Activa es regla de negocio.
        Task<List<PedidoAD>> ObtenerOrdenesFacturables();

        // Orden puntual (con Cliente) para validar/leer datos al emitir.
        Task<PedidoAD?> ObtenerPedidoParaFacturar(int pedidoId);

        // IdUsuario de dbo.Usuario cuyo Correo coincida (case-insensitive), o null.
        Task<int?> ObtenerIdUsuarioPorCorreo(string correo);
    }
}
