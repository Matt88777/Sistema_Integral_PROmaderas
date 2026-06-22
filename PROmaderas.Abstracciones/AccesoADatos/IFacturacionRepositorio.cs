using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IFacturacionRepositorio
    {
        // Listado del Index: facturas activas con Pedido y Cliente, más recientes primero.
        Task<List<FacturacionAD>> ObtenerTodasActivas();

        // Listado del Index con filtros opcionales (cliente, rango de fechas, número consecutivo).
        Task<List<FacturacionAD>> BuscarConFiltros(int? clienteId, DateTime? fechaDesde,
                                                   DateTime? fechaHasta, string? numeroFactura);

        // Detalle: factura con Pedido -> Detalles -> Producto y Cliente.
        Task<FacturacionAD?> ObtenerPorId(int id);

        // Inserta la factura y guarda.
        Task<FacturacionAD> Crear(FacturacionAD factura);

        // Cambia SOLO el campo Estado y registra la bitácora en el mismo SaveChanges (atómico).
        Task CambiarEstado(int id, string nuevoEstado, ContextoAuditoria auditoria);

        // Mayor Id existente (0 si no hay) para calcular el consecutivo.
        Task<int> ObtenerMaximoId();

        // ¿La orden ya tiene una factura activa?
        Task<bool> PedidoYaFacturado(int pedidoId);

        // Órdenes que se pueden facturar: activas, no canceladas y aún sin factura activa.
        Task<List<PedidoAD>> ObtenerOrdenesFacturables();

        // Orden puntual (con Cliente) para validar/leer datos al emitir.
        Task<PedidoAD?> ObtenerPedidoParaFacturar(int pedidoId);

        // IdUsuario de dbo.Usuario cuyo Correo coincida (case-insensitive), o null.
        Task<int?> ObtenerIdUsuarioPorCorreo(string correo);
    }
}
