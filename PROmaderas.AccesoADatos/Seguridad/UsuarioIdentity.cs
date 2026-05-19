using Microsoft.AspNetCore.Identity;

namespace PROmaderas.AccesoADatos.Seguridad
{
	public class UsuarioIdentity : IdentityUser
	{
		public string NombreCompleto { get; set; } = string.Empty;
	}
}
