using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
    public class PerfilViewModel
    {
        [Display(Name = "Correo")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = "";

        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Rol")]
        public string Rol { get; set; } = "";
    }
}
