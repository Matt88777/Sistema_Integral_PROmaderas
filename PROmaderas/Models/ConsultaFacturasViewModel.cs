using PROmaderas.Abstracciones.Models;

namespace PROmaderas.UI.Models
{
    public class ConsultaFacturasViewModel
    {
        // Filtros (entrada)
        public int? ClienteId { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? NumeroFactura { get; set; }

        // FAC-HU-005: solo el Administrador puede activarlo; el Controller lo fuerza a false
        // para el resto de roles, así que refleja lo que REALMENTE se consultó.
        public bool IncluirInactivas { get; set; }

        // Resultados (salida)
        public List<FacturacionAD> Facturas { get; set; } = new();
        public List<ClienteAD> Clientes { get; set; } = new();
    }
}
