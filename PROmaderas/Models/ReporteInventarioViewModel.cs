using PROmaderas.Abstracciones.Models;

namespace PROmaderas.UI.Models
{
    // REP-HU-002: ViewModel de la pantalla de Reporte de Inventario.
    public class ReporteInventarioViewModel
    {
        public bool ConsultaRealizada { get; set; }
        public string? MensajeError { get; set; }
        public ReporteInventarioResultadoDTO? Resultado { get; set; }
    }
}