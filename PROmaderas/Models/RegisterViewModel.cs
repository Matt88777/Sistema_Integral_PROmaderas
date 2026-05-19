using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "El nombre es requerido")]
		[StringLength(150)]
		[Display(Name = "Nombre")]
		public string Nombre { get; set; } = string.Empty;

		[Required(ErrorMessage = "La cédula es requerida")]
		[StringLength(20)]
		[RegularExpression(@"^\d{6,20}$", ErrorMessage = "La cédula debe contener sólo dígitos (6-20 caracteres)")]
		[Display(Name = "Cédula")]
		public string Cedula { get; set; } = string.Empty;

		[Required(ErrorMessage = "El email es requerido")]
		[EmailAddress(ErrorMessage = "Ingrese un email válido")]
		[Display(Name = "Correo Electrónico")]
		public string Email { get; set; } = string.Empty;

		[StringLength(20)]
		[RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Teléfono inválido. Use sólo dígitos, opcionalmente con + y entre 7 y 15 caracteres")]
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

		[Required(ErrorMessage = "El rol es requerido")]
		[Display(Name = "Rol")]
		public string Rol { get; set; } = "Cliente";

		public string? ReturnUrl { get; set; }
	}
}
