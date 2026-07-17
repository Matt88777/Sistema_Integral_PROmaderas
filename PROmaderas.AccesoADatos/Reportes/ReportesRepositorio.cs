using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.Models;
using System.Globalization;

namespace PROmaderas.AccesoADatos.Reportes
{
    public class ReportesRepositorio : IReportesRepositorio
    {
        private readonly Contexto _contexto;
        private readonly IProductoRepositorio _productoRepositorio;

        public ReportesRepositorio(Contexto contexto, IProductoRepositorio productoRepositorio)
        {
            _contexto = contexto;
            _productoRepositorio = productoRepositorio;
        }

        // REP-HU-001 - Escenarios 1 y 2: ventas (pedidos activos) agrupadas por periodo,
        // con el estado de facturación de cada pedido (última factura activa asociada).
        // El agrupamiento semanal/mensual no es trivial de traducir a SQL, así que los
        // pedidos del rango se traen a memoria (volumen acotado por el rango de fechas)
        // y se agrupan con LINQ-to-Objects, igual que ObtenerHistorialMovimientos.
        public async Task<List<VentaPeriodoDTO>> ObtenerVentasPorPeriodo(string tipoPeriodo, DateTime fechaInicio, DateTime fechaFin)
        {
            var fechaFinInclusive = fechaFin.Date.AddDays(1).AddTicks(-1);

            var pedidosBase = await (
                from pedido in _contexto.Pedidos.AsNoTracking()
                where pedido.Activa
                      && pedido.Fecha >= fechaInicio.Date
                      && pedido.Fecha <= fechaFinInclusive
                select new
                {
                    pedido.Id,
                    pedido.Fecha,
                    pedido.Total,
                    EstadoFactura = _contexto.Facturaciones
                        .Where(f => f.PedidoId == pedido.Id && f.Activa)
                        .OrderByDescending(f => f.Fecha)
                        .Select(f => f.Estado)
                        .FirstOrDefault()
                }
            ).ToListAsync();

            var resultado = pedidosBase
                .GroupBy(p => ClavePeriodo(p.Fecha, tipoPeriodo))
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var (inicio, fin, etiqueta) = DescribirPeriodo(g.Key, tipoPeriodo);
                    return new VentaPeriodoDTO
                    {
                        Periodo = etiqueta,
                        FechaInicio = inicio,
                        FechaFin = fin,
                        CantidadPedidos = g.Count(),
                        MontoTotal = g.Sum(x => x.Total),
                        FacturasEmitidas = g.Count(x => x.EstadoFactura == EstadosFactura.Emitida),
                        FacturasPagadas = g.Count(x => x.EstadoFactura == EstadosFactura.Pagada),
                        FacturasPendientes = g.Count(x => x.EstadoFactura == EstadosFactura.PendienteDePago),
                        FacturasAnuladas = g.Count(x => x.EstadoFactura == EstadosFactura.Anulada),
                        PedidosSinFacturar = g.Count(x => x.EstadoFactura == null)
                    };
                })
                .ToList();

            return resultado;
        }

        private static DateTime ClavePeriodo(DateTime fecha, string tipoPeriodo)
        {
            return tipoPeriodo switch
            {
                PeriodosReporteVentas.Semanal => InicioSemana(fecha),
                PeriodosReporteVentas.Mensual => new DateTime(fecha.Year, fecha.Month, 1),
                _ => fecha.Date // Diario
            };
        }

        private static DateTime InicioSemana(DateTime fecha)
        {
            // Semana inicia el lunes (convención CR).
            int diferencia = (7 + (fecha.DayOfWeek - DayOfWeek.Monday)) % 7;
            return fecha.Date.AddDays(-diferencia);
        }

        private static (DateTime inicio, DateTime fin, string etiqueta) DescribirPeriodo(DateTime clave, string tipoPeriodo)
        {
            var cultura = CultureInfo.GetCultureInfo("es-CR");

            switch (tipoPeriodo)
            {
                case PeriodosReporteVentas.Semanal:
                    var finSemana = clave.AddDays(6);
                    return (clave, finSemana, $"Semana del {clave:dd/MM/yyyy} al {finSemana:dd/MM/yyyy}");

                case PeriodosReporteVentas.Mensual:
                    var finMes = clave.AddMonths(1).AddDays(-1);
                    var etiquetaMes = cultura.TextInfo.ToTitleCase(clave.ToString("MMMM yyyy", cultura));
                    return (clave, finMes, etiquetaMes);

                default: // Diario
                    return (clave, clave, clave.ToString("dd/MM/yyyy"));
            }
        }

        // REP-HU-002 - Escenario 1: existencias actuales (entradas, salidas, stock actual y mínimo)
        // por tipo de tarima. Se reutiliza la consulta de ProductoRepositorio (INV-HU) para no
        // duplicar la lógica de cálculo de stock a partir de InventarioMovimiento.
        public async Task<List<InventarioExistenciaDTO>> ObtenerExistenciasInventario()
        {
            return await _productoRepositorio.ObtenerExistenciasActuales(idTipoTarima: null);
        }

        // REP-HU-002 - Escenario 2: movimientos de inventario (entradas y salidas), todos los tipos.
        public async Task<List<InventarioMovimientoDTO>> ObtenerMovimientosInventario()
        {
            return await _productoRepositorio.ObtenerHistorialMovimientos(idTipoTarima: null);
        }

        public async Task<List<FacturacionAD>> ObtenerFacturas()
        {
            return await _contexto.Facturaciones
                .Include(f => f.Cliente)
                .Where(f => f.Activa)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();
        }

        public async Task<List<ProductoAD>> ObtenerProductos()
        {
            return await _contexto.Productos
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<List<PlanillaDetalleFinancieroAD>> ObtenerPlanillaDetalles()
        {
            return await _contexto.PlanillaDetallesFinancieros
                .Include(d => d.Periodo)
                .Include(d => d.Empleado)
                .OrderByDescending(d => d.Periodo!.FechaInicio)
                .ToListAsync();
        }
    }
}
//private readonly Contexto _contexto;
//private readonly IProductoRepositorio _productoRepositorio;

//public ReportesRepositorio(Contexto contexto, IProductoRepositorio productoRepositorio)
// {
//     _contexto = contexto;
//     _productoRepositorio = productoRepositorio;
// }

// public async Task<List<FacturacionAD>> ObtenerFacturas()
// {
//     return await _contexto.Facturaciones
//         .Include(f => f.Cliente)
//         .Where(f => f.Activa)
//         .OrderByDescending(f => f.Fecha)
//         .ToListAsync();
// }

// public async Task<List<ProductoAD>> ObtenerProductos()
// {
//     return await _contexto.Productos
//       .Where(p => p.Activo)
//     .OrderBy(p => p.Nombre)
//   .ToListAsync();
//}

//public async Task<List<PlanillaDetalleFinancieroAD>> ObtenerPlanillaDetalles()
//{
//    return await _contexto.PlanillaDetallesFinancieros
//       .Include(d => d.Periodo)
//     .Include(d => d.Empleado)
//   .OrderByDescending(d => d.Periodo!.FechaInicio)
// .ToListAsync();
// }
//codigo nuevo
// public async Task<List<InventarioExistenciaDTO>> ObtenerExistenciasInventario()
// {
//     return await _productoRepositorio.ObtenerExistenciasActuales(idTipoTarima: null);
// }

// REP-HU-002 - Escenario 2: movimientos de inventario (entradas y salidas), todos los tipos.
// public async Task<List<InventarioMovimientoDTO>> ObtenerMovimientosInventario()
// {
//     return await _productoRepositorio.ObtenerHistorialMovimientos(idTipoTarima: null);
// }


// }
