using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models;

public class AdminResetPasswordViewModel
{
    public string UserId { get; set; } = string.Empty;

    [Display(Name = "Correo")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contrasena es requerida")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contrasena")]
    public string NuevaContrasena { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contrasena")]
    [Compare(nameof(NuevaContrasena), ErrorMessage = "La contrasena y la confirmacion no coinciden")]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
