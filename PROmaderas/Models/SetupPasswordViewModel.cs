using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models;

public class SetupPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contrasena es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contrasena debe tener entre 6 y 100 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contrasena")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe confirmar la contrasena")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contrasena")]
    [Compare("Password", ErrorMessage = "La contrasena y la confirmacion no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
