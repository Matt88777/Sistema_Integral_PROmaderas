using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    public interface IFacturacionLogica
    {
        // Index: listado de facturas activas.
        Task<List<FacturacionAD>> ObtenerListado();

        // Index con filtros: cliente, rango de fechas y número consecutivo (todos opcionales).
        Task<List<FacturacionAD>> BuscarConFiltros(int? clienteId, DateTime? fechaDesde,
                                                   DateTime? fechaHasta, string? numeroFactura);

        // Details: factura por Id con sus relaciones.
        Task<FacturacionAD?> ObtenerDetalle(int id);

        // FAC-HU-003: cambia el estado de la factura (valida catálogo y que sea distinto del actual).
        Task CambiarEstado(int id, string nuevoEstado, ContextoAuditoria auditoria);

        // FAC-HU-004: registra un pago (parcial o total), baja el saldo y ajusta el estado.
        Task RegistrarPago(int idFactura, decimal monto, string formaPago, string? referencia,
                           DateTime fechaPago, string correoOperador, ContextoAuditoria auditoria);

        // Historial de pagos de una factura (para Details).
        Task<List<PagoFacturaAD>> ObtenerPagosPorFactura(int idFactura);

        // Emite la factura: resuelve el emisor por correo, genera el consecutivo,
        // fija Estado = "Emitida", SaldoPendiente = Total y persiste.
        Task<FacturacionAD> Crear(FacturacionAD factura, string correoEmisor);

        // ¿La orden ya tiene una factura activa? (validación del Create POST).
        Task<bool> PedidoYaFacturado(int pedidoId);

        // Órdenes facturables para el dropdown del Create.
        Task<List<PedidoAD>> ObtenerOrdenesFacturables();

        // Orden puntual (con Cliente) para leer datos al emitir.
        Task<PedidoAD?> ObtenerPedidoParaFacturar(int pedidoId);
    }
}
