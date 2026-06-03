using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(150)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Cédula")]
        public string? Cedula { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Ingrese un email válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Teléfono inválido.")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(300)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Rol")]
        public string? Rol { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
