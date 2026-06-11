using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
    public class CreateFacturaViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una orden de compra.")]
        public int PedidoId { get; set; }
        public string? NumeroOrden { get; set; }
        public string? ClienteNombre { get; set; }
        public bool Exonerado { get; set; }
        public decimal PorcentajeExo { get; set; }
        public decimal SubtotalOC { get; set; }
        public decimal ExoneracionCalc { get; set; }
        public decimal ImpuestoCalc { get; set; }
        public decimal TotalCalc { get; set; }
    }
}