using System.Text.Json;
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

        public async Task<List<FacturacionAD>> ObtenerListado(bool incluirInactivas = false)
        {
            return await _repositorio.ObtenerTodas(incluirInactivas);
        }

        public async Task<List<FacturacionAD>> BuscarConFiltros(int? clienteId, DateTime? fechaDesde,
                                                                DateTime? fechaHasta, string? numeroFactura,
                                                                bool incluirInactivas = false)
        {
            // Normalizamos el número acá (no en el repo): así el repo recibe ya sea null o texto limpio.
            var numero = string.IsNullOrWhiteSpace(numeroFactura) ? null : numeroFactura.Trim();
            return await _repositorio.BuscarConFiltros(clienteId, fechaDesde, fechaHasta, numero, incluirInactivas);
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

        public async Task Inactivar(int id, string motivo, ContextoAuditoria auditoria)
        {
            var factura = await _repositorio.ObtenerPorId(id);
            if (factura == null)
                throw new ArgumentException("La factura no existe");

            if (!factura.Activa)
                throw new ArgumentException("La factura ya está inactiva.");

            // Se permite inactivar una factura con pagos (la vista advierte antes de confirmar):
            // se corrige el error sin borrar la evidencia, el pago sigue en PagoFactura.
            await _repositorio.CambiarActiva(id, false, EstadosFactura.Anulada,
                                             ValidarMotivo(motivo), auditoria);
        }

        public async Task Reactivar(int id, string motivo, ContextoAuditoria auditoria)
        {
            var factura = await _repositorio.ObtenerPorId(id);
            if (factura == null)
                throw new ArgumentException("La factura no existe");

            if (factura.Activa)
                throw new ArgumentException("La factura ya está activa.");

            // Mientras estuvo anulada, la orden volvió a ser facturable y pudieron emitirle otra
            // factura. Reactivar a ciegas dejaría DOS facturas activas para la misma orden.
            var impedimento = await BuscarImpedimentoReactivacion(factura);
            if (impedimento != null)
                throw new ArgumentException(impedimento);

            var pagos = await _repositorio.ObtenerPagosPorFactura(id);
            var estado = RecalcularEstado(factura.SaldoPendiente, pagos.Count);

            await _repositorio.CambiarActiva(id, true, estado, ValidarMotivo(motivo), auditoria);
        }

        public async Task<string?> ObtenerImpedimentoReactivacion(int idFactura)
        {
            var factura = await _repositorio.ObtenerPorId(idFactura);
            if (factura == null || factura.Activa)
                return null;

            return await BuscarImpedimentoReactivacion(factura);
        }

        // Devuelve el mensaje del bloqueo, o null si la factura se puede reactivar.
        // Lo usan Reactivar (para cortar el POST) y la pantalla de confirmación (para avisar antes).
        private async Task<string?> BuscarImpedimentoReactivacion(FacturacionAD factura)
        {
            var enConflicto = await _repositorio.ObtenerOtraFacturaActivaDeOrden(
                factura.PedidoId, factura.Id);

            if (enConflicto == null)
                return null;

            var orden = enConflicto.Pedido?.NumeroOrden
                        ?? factura.Pedido?.NumeroOrden
                        ?? $"#{factura.PedidoId}";

            return $"No se puede reactivar: la orden {orden} ya tiene la factura " +
                   $"{enConflicto.NumeroFactura} activa. Debe inactivar esa factura primero.";
        }

        public async Task<string?> ObtenerMotivoAnulacion(int idFactura)
        {
            var bitacora = await _repositorio.ObtenerUltimaBitacoraFactura(
                idFactura, AccionesAuditoria.InactivarFactura);

            if (string.IsNullOrWhiteSpace(bitacora?.ValorNuevo))
                return null;

            // Forma del JSON que arma ConstructorBitacora: { modificadoPor: {...}, datos: {...} }.
            try
            {
                using var json = JsonDocument.Parse(bitacora.ValorNuevo);
                if (json.RootElement.TryGetProperty("datos", out var datos) &&
                    datos.TryGetProperty("Motivo", out var motivo))
                {
                    return motivo.GetString();
                }
            }
            catch (JsonException)
            {
                // Bitácora vieja o con otro formato: el Details igual muestra el banner, sin motivo.
            }

            return null;
        }

        // Al reactivar el Estado NO se adivina: se deriva de los datos de la propia factura.
        private static string RecalcularEstado(decimal saldoPendiente, int cantidadPagos)
        {
            if (saldoPendiente <= 0)
                return EstadosFactura.Pagada;

            return cantidadPagos > 0
                ? EstadosFactura.PendienteDePago   // hay pagos pero queda saldo
                : EstadosFactura.Emitida;          // nunca se pagó nada
        }

        // El motivo es la justificación de la autorización: sin él no se guarda el cambio.
        private static string ValidarMotivo(string motivo)
        {
            var limpio = (motivo ?? string.Empty).Trim();
            if (limpio.Length == 0)
                throw new ArgumentException("Debe indicar el motivo del cambio.");

            return limpio;
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
