using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "El correo es requerido")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;
}
