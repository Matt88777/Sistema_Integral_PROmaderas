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
