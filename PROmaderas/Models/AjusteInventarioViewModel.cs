using PROmaderas.Abstracciones.Models;

namespace PROmaderas.UI.Models
{
    public class AjusteInventarioViewModel
    {
        public AjusteInventarioDTO Ajuste { get; set; } = new();
        public List<ProductoAD> TiposTarima { get; set; } = new();
        public List<InventarioExistenciaDTO> Existencias { get; set; } = new();
    }
}
