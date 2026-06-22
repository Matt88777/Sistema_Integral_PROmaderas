using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    public interface IFacturacionRepositorio
    {
        // Listado del Index: facturas activas con Pedido y Cliente, más recientes primero.
        Task<List<FacturacionAD>> ObtenerTodasActivas();

        // Detalle: factura con Pedido -> Detalles -> Producto y Cliente.
        Task<FacturacionAD?> ObtenerPorId(int id);

        // Inserta la factura y guarda.
        Task<FacturacionAD> Crear(FacturacionAD factura);

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
