using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROmaderas.Abstracciones.Models
{
    public class InventarioMovimientoDTO
    {
        public int IdMovimiento { get; set; }
        public int IdTipoTarima { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string TipoTarima { get; set; } = string.Empty;
        public string Medida { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public int Saldo { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string? Motivo { get; set; }
        public int? IdProduccion { get; set; }
        public int? IdOrdenCompra { get; set; }
    }
}
