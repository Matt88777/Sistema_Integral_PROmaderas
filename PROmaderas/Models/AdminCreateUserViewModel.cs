using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models;

public class AdminCreateUserViewModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(150)]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Ingrese un email valido")]
    [Display(Name = "Correo electronico")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    [RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Telefono invalido. Use solo digitos, opcionalmente con + y entre 7 y 15 caracteres")]
    [Display(Name = "Telefono")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El rol es requerido")]
    [Display(Name = "Rol interno")]
    public string Rol { get; set; } = "Vendedor";
}
