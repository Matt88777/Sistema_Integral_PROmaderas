using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PROmaderas.AccesoADatos.Seguridad;

namespace PROmaderas.AccesoADatos
{
	public class ApplicationDbContext : IdentityDbContext<UsuarioIdentity>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
	}
}