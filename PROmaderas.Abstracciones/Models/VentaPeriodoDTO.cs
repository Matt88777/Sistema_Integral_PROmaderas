namespace PROmaderas.Abstracciones.Models
{
    public class VentaPeriodoDTO
    {
        public string Periodo { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public int CantidadPedidos { get; set; }
        public decimal MontoTotal { get; set; }

        public int FacturasEmitidas { get; set; }
        public int FacturasPagadas { get; set; }
        public int FacturasPendientes { get; set; }
        public int FacturasAnuladas { get; set; }
        public int PedidosSinFacturar { get; set; }
    }
}
