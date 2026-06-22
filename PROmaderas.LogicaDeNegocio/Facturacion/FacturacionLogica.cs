using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
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

        public async Task<List<FacturacionAD>> BuscarConFiltros(int? clienteId, DateTime? fechaDesde,
                                                                DateTime? fechaHasta, string? numeroFactura)
        {
            // Normalizamos el número acá (no en el repo): así el repo recibe ya sea null o texto limpio.
            var numero = string.IsNullOrWhiteSpace(numeroFactura) ? null : numeroFactura.Trim();
            return await _repositorio.BuscarConFiltros(clienteId, fechaDesde, fechaHasta, numero);
        }

        public async Task<FacturacionAD?> ObtenerDetalle(int id)
        {
            return await _repositorio.ObtenerPorId(id);
        }

        public async Task CambiarEstado(int id, string nuevoEstado, ContextoAuditoria auditoria)
        {
            var factura = await _repositorio.ObtenerPorId(id);
            if (factura == null)
                throw new ArgumentException("La factura no existe");

            // FAC-HU-003: solo se permiten estos dos estados (Pagada/Anulada son de otras HU).
            if (nuevoEstado != EstadosFactura.Emitida && nuevoEstado != EstadosFactura.PendienteDePago)
                throw new ArgumentException("El estado seleccionado no es válido.");

            if (nuevoEstado == factura.Estado)
                throw new ArgumentException("La factura ya está en ese estado.");

            await _repositorio.CambiarEstado(id, nuevoEstado, auditoria);
        }

        public async Task RegistrarPago(int idFactura, decimal monto, string formaPago, string? referencia,
                                        DateTime fechaPago, string correoOperador, ContextoAuditoria auditoria)
        {
            var factura = await _repositorio.ObtenerPorId(idFactura);
            if (factura == null)
                throw new ArgumentException("La factura no existe");

            if (!factura.Activa)
                throw new ArgumentException("No se puede registrar el pago de una factura inactiva.");

            if (factura.Estado == EstadosFactura.Pagada || factura.SaldoPendiente <= 0)
                throw new ArgumentException("La factura ya está pagada.");

            if (monto <= 0)
                throw new ArgumentException("El monto debe ser mayor a cero.");

            if (monto > factura.SaldoPendiente)
                throw new ArgumentException($"El monto excede el saldo pendiente (₡{factura.SaldoPendiente:N2}).");

            if (!FormasPago.Todas.Contains(formaPago))
                throw new ArgumentException("Forma de pago no válida.");

            // Resolver el usuario que registra (FK NOT NULL). La identidad llega por parámetro,
            // no se toca Identity acá. Fallback a admin (IdUsuario = 1) como en la emisión.
            var idUsuario = await _repositorio.ObtenerIdUsuarioPorCorreo(correoOperador) ?? 1;

            var pago = new PagoFacturaAD
            {
                IdFactura = idFactura,
                Monto = monto,
                FormaPago = formaPago,
                Referencia = referencia,
                FechaPago = fechaPago,
                IdUsuarioRegistro = idUsuario
            };

            await _repositorio.RegistrarPago(pago, auditoria);
        }

        public async Task<List<PagoFacturaAD>> ObtenerPagosPorFactura(int idFactura)
        {
            return await _repositorio.ObtenerPagosPorFactura(idFactura);
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
