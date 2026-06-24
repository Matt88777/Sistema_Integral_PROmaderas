namespace PROmaderas.Abstracciones.Models
{
	public class DashboardFinancieroDTO
	{
		public bool HayDatos { get; set; }

		public decimal TotalIngresos { get; set; }
		public decimal TotalEgresos { get; set; }

		public decimal Balance => TotalIngresos - TotalEgresos;

		public decimal TotalFacturado { get; set; }
		public decimal SaldoPendiente { get; set; }

		public int FacturasEmitidas { get; set; }
		public int FacturasPagadas { get; set; }
		public int FacturasPendientes { get; set; }

		public int OrdenesActivas { get; set; }
		public int ClientesActivos { get; set; }
		public int ProductosActivos { get; set; }

		public DateTime FechaUltimaActualizacion { get; set; } = DateTime.Now;

		public List<DashboardMesDTO> ResumenMensual { get; set; } = new();
	}

	public class DashboardMesDTO
	{
		public string Mes { get; set; } = string.Empty;
		public decimal Ingresos { get; set; }
		public decimal Egresos { get; set; }
		public decimal Balance => Ingresos - Egresos;
	}
}