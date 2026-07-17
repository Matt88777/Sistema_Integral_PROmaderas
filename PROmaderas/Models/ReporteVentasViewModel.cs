using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.UI.Models
{
    public class ReporteVentasViewModel
    {
        public string TipoPeriodo { get; set; } = PeriodosReporteVentas.Mensual;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public VentasReporteResultadoDTO? Resultado { get; set; }
        public bool ConsultaRealizada { get; set; }
        public string? MensajeError { get; set; }
    }
}