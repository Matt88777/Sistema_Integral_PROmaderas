using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROmaderas.Abstracciones.Models
{
    public class InventarioExistenciaDTO
    {
        public int IdTipoTarima { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string TipoTarima { get; set; } = string.Empty;
        public string Medida { get; set; } = string.Empty;
        public int Entradas { get; set; }
        public int Salidas { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
    }
}
