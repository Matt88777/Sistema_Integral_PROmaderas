namespace PROmaderas.Abstracciones.Models
{
    public class VentasReporteResultadoDTO
    {
        public string TipoPeriodo { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public List<VentaPeriodoDTO> Periodos { get; set; } = new();

        public int CantidadPedidosGeneral => Periodos.Sum(p => p.CantidadPedidos);
        public decimal MontoTotalGeneral => Periodos.Sum(p => p.MontoTotal);

        /// <summary>Escenario 3: no existen ventas registradas en el rango/periodo consultado.</summary>
        public bool HayDatos => Periodos.Count > 0;
    }
}
