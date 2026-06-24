using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.Models;
using System.Globalization;

namespace PROmaderas.AccesoADatos.Dashboard
{
	public class DashboardRepositorio : IDashboardRepositorio
	{
		private readonly Contexto _contexto;

		public DashboardRepositorio(Contexto contexto)
		{
			_contexto = contexto;
		}

		public async Task<DashboardFinancieroDTO> ObtenerDashboardFinancieroAsync()
		{
			var modelo = new DashboardFinancieroDTO
			{
				FechaUltimaActualizacion = DateTime.Now
			};

			var facturasValidas = _contexto.Facturaciones
				.AsNoTracking()
				.Where(f => f.Activa && f.Estado != EstadosFactura.Anulada);

			var planillaPagada =
				from detalle in _contexto.PlanillaDetallesFinancieros.AsNoTracking()
				join periodo in _contexto.PlanillaPeriodos.AsNoTracking()
					on detalle.IdPlanillaPeriodo equals periodo.IdPlanillaPeriodo
				where periodo.Estado == "Pagada"
				select new
				{
					periodo.FechaFin,
					detalle.SalarioNeto
				};

			modelo.TotalIngresos = await facturasValidas
				.SumAsync(f => (decimal?)f.Total) ?? 0;

			modelo.TotalFacturado = modelo.TotalIngresos;

			modelo.SaldoPendiente = await facturasValidas
				.SumAsync(f => (decimal?)f.SaldoPendiente) ?? 0;

			modelo.TotalEgresos = await planillaPagada
				.SumAsync(p => (decimal?)p.SalarioNeto) ?? 0;

			modelo.FacturasEmitidas = await facturasValidas.CountAsync();

			modelo.FacturasPagadas = await facturasValidas
				.CountAsync(f => f.Estado == EstadosFactura.Pagada);

			modelo.FacturasPendientes = await facturasValidas
				.CountAsync(f => f.Estado == EstadosFactura.PendienteDePago || f.SaldoPendiente > 0);

			modelo.OrdenesActivas = await _contexto.Pedidos
				.AsNoTracking()
				.CountAsync(p => p.Activa);

			modelo.ClientesActivos = await _contexto.Clientes
				.AsNoTracking()
				.CountAsync(c => c.Estado);

			modelo.ProductosActivos = await _contexto.Productos
				.AsNoTracking()
				.CountAsync(p => p.Activo);

			modelo.ResumenMensual = await ObtenerResumenMensualAsync();

			modelo.HayDatos =
				modelo.FacturasEmitidas > 0 ||
				modelo.TotalEgresos > 0 ||
				modelo.OrdenesActivas > 0 ||
				modelo.ClientesActivos > 0 ||
				modelo.ProductosActivos > 0;

			return modelo;
		}

		private async Task<List<DashboardMesDTO>> ObtenerResumenMensualAsync()
		{
			var cultura = CultureInfo.GetCultureInfo("es-CR");

			var fechaInicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
				.AddMonths(-5);

			var ingresosPorMes = await _contexto.Facturaciones
				.AsNoTracking()
				.Where(f => f.Activa &&
							f.Estado != EstadosFactura.Anulada &&
							f.Fecha >= fechaInicio)
				.GroupBy(f => new { f.Fecha.Year, f.Fecha.Month })
				.Select(g => new
				{
					g.Key.Year,
					g.Key.Month,
					Total = g.Sum(x => x.Total)
				})
				.ToListAsync();

			var egresosPorMes = await (
				from detalle in _contexto.PlanillaDetallesFinancieros.AsNoTracking()
				join periodo in _contexto.PlanillaPeriodos.AsNoTracking()
					on detalle.IdPlanillaPeriodo equals periodo.IdPlanillaPeriodo
				where periodo.Estado == "Pagada" && periodo.FechaFin >= fechaInicio
				group detalle by new { periodo.FechaFin.Year, periodo.FechaFin.Month }
				into g
				select new
				{
					g.Key.Year,
					g.Key.Month,
					Total = g.Sum(x => x.SalarioNeto)
				})
				.ToListAsync();

			var resumen = new List<DashboardMesDTO>();

			for (var fecha = fechaInicio; fecha <= DateTime.Today; fecha = fecha.AddMonths(1))
			{
				var ingreso = ingresosPorMes
					.FirstOrDefault(x => x.Year == fecha.Year && x.Month == fecha.Month)?.Total ?? 0;

				var egreso = egresosPorMes
					.FirstOrDefault(x => x.Year == fecha.Year && x.Month == fecha.Month)?.Total ?? 0;

				resumen.Add(new DashboardMesDTO
				{
					Mes = cultura.TextInfo.ToTitleCase(fecha.ToString("MMMM yyyy", cultura)),
					Ingresos = ingreso,
					Egresos = egreso
				});
			}

			return resumen;
		}
	}
}