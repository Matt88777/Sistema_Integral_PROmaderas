using System.ComponentModel.DataAnnotations;

namespace PROmaderas.Abstracciones.Models
{
    public class AjusteInventarioDTO
    {
        [Required(ErrorMessage = "Debe seleccionar un tipo de tarima.")]
        [Display(Name = "Tipo de tarima")]
        public int IdTipoTarima { get; set; }

        [Required(ErrorMessage = "Debe indicar el tipo de ajuste.")]
        [Display(Name = "Tipo de ajuste")]
        public string TipoAjuste { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "Debe registrar el motivo del ajuste.")]
        [StringLength(180, ErrorMessage = "El motivo no puede superar los 180 caracteres.")]
        [Display(Name = "Motivo")]
        public string Motivo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar la autorización del ajuste.")]
        [StringLength(60, ErrorMessage = "La autorización no puede superar los 60 caracteres.")]
        [Display(Name = "Autorización")]
        public string Autorizacion { get; set; } = string.Empty;

        public string? CorreoUsuarioRegistro { get; set; }
    }
}