using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    public interface IFacturacionLogica
    {
        // Index: listado de facturas (activas por defecto).
        Task<List<FacturacionAD>> ObtenerListado(bool incluirInactivas = false);

        // Index con filtros: cliente, rango de fechas y número consecutivo (todos opcionales).
        // FAC-HU-005: incluirInactivas lo decide el Controller (solo Administrador).
        Task<List<FacturacionAD>> BuscarConFiltros(int? clienteId, DateTime? fechaDesde,
                                                   DateTime? fechaHasta, string? numeroFactura,
                                                   bool incluirInactivas = false);

        // Details: factura por Id con sus relaciones (también si está inactiva).
        Task<FacturacionAD?> ObtenerDetalle(int id);

        // FAC-HU-003: cambia el estado de la factura (valida catálogo y que sea distinto del actual).
        Task CambiarEstado(int id, string nuevoEstado, ContextoAuditoria auditoria);

        // FAC-HU-005: Activa = false + Estado = Anulada. Motivo obligatorio (va a la bitácora).
        // Se permite aunque la factura tenga pagos: la advertencia es responsabilidad de la vista.
        Task Inactivar(int id, string motivo, ContextoAuditoria auditoria);

        // FAC-HU-005: Activa = true y el Estado se RECALCULA desde los datos (no se adivina).
        // Motivo obligatorio (va a la bitácora). Falla si la orden ya tiene otra factura activa.
        Task Reactivar(int id, string motivo, ContextoAuditoria auditoria);

        // FAC-HU-005: motivo por el que la factura NO se puede reactivar (otra factura activa
        // para la misma orden), o null si sí se puede. La pantalla de confirmación avisa con esto
        // antes de que el Administrador escriba el motivo.
        Task<string?> ObtenerImpedimentoReactivacion(int idFactura);

        // FAC-HU-005: motivo de la última anulación (para el banner del Details). Null si no hay.
        Task<string?> ObtenerMotivoAnulacion(int idFactura);

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
