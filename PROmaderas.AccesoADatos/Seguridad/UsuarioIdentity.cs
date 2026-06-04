using Microsoft.AspNetCore.Identity;

namespace PROmaderas.AccesoADatos.Seguridad
{
	public class UsuarioIdentity : IdentityUser
	{
		public int? IdEmpleado { get; set; }
		public string NombreCompleto { get; set; } = string.Empty;
	}
}
