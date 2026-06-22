using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Facturacion
{
    public class FacturacionLogica : IFacturacionLogica
    {
        private readonly IFacturacionRepositorio _repositorio;

        public FacturacionLogica(IFacturacionRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<FacturacionAD>> ObtenerListado()
        {
            return await _repositorio.ObtenerTodasActivas();
        }

        public async Task<FacturacionAD?> ObtenerDetalle(int id)
        {
            return await _repositorio.ObtenerPorId(id);
        }

        public async Task<FacturacionAD> Crear(FacturacionAD factura, string correoEmisor)
        {
            // Regla de negocio (antes en el Create POST del controller):
            // consecutivo FAC-yyyyMMdd-0001 con MAX(Id)+1, estado inicial "Emitida".
            var maxId = await _repositorio.ObtenerMaximoId();
            var ahora = DateTime.Now;

            // Emisor: resolver IdUsuario por correo; fallback a admin (IdUsuario = 1).
            var idEmisor = await _repositorio.ObtenerIdUsuarioPorCorreo(correoEmisor);
            factura.IdUsuarioEmisor = idEmisor ?? 1;

            // Al emitir no hay pagos aún: el saldo pendiente es el total.
            factura.SaldoPendiente = factura.Total;

            factura.NumeroFactura = $"FAC-{ahora:yyyyMMdd}-{(maxId + 1):D4}";
            factura.Fecha = ahora;
            factura.Estado = "Emitida";
            factura.Activa = true;

            return await _repositorio.Crear(factura);
        }

        public async Task<bool> PedidoYaFacturado(int pedidoId)
        {
            return await _repositorio.PedidoYaFacturado(pedidoId);
        }

        public async Task<List<PedidoAD>> ObtenerOrdenesFacturables()
        {
            return await _repositorio.ObtenerOrdenesFacturables();
        }

        public async Task<PedidoAD?> ObtenerPedidoParaFacturar(int pedidoId)
        {
            return await _repositorio.ObtenerPedidoParaFacturar(pedidoId);
        }
    }
}
