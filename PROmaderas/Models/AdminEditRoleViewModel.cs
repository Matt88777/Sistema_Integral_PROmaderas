using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models;

public class AdminEditRoleViewModel
{
	public string UserId { get; set; } = string.Empty;

	[Display(Name = "Nombre completo")]
	public string Nombre { get; set; } = string.Empty;

	[Display(Name = "Correo electronico")]
	public string Email { get; set; } = string.Empty;

	[Required(ErrorMessage = "El rol es requerido")]
	[Display(Name = "Rol del usuario")]
	public string Rol { get; set; } = string.Empty;
}