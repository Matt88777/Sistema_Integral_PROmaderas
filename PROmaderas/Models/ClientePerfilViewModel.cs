using System.ComponentModel.DataAnnotations;

namespace PROmaderas.UI.Models
{
	public class ClientePerfilViewModel
	{
		[Required]
		public string Nombre { get; set; } = string.Empty;

		[Required]
		public string Cedula { get; set; } = string.Empty;

		[Required]
		[EmailAddress]
		public string Correo { get; set; } = string.Empty;

		[Phone]
		public string? Telefono { get; set; }

		public string? Direccion { get; set; }
	}
}