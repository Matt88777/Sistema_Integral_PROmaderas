using PROmaderas.Abstracciones.Models;

namespace PROmaderas.UI.Models
{
    public class InventarioConsultaViewModel
    {
        public int? IdTipoTarima { get; set; }
        public List<ProductoAD> TiposTarima { get; set; } = new();
        public List<InventarioExistenciaDTO> Existencias { get; set; } = new();
        public List<InventarioMovimientoDTO> Movimientos { get; set; } = new();
        public bool ConsultaRealizada { get; set; }
    }
}