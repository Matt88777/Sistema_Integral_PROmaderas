using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace PROmaderas.AccesoADatos.Seguridad
{
	public static class IdentitySeeder
	{
		public static readonly string[] RolesBase =
		[
			"Administrador",
			"Gerente",
			"Contador",
			"Operador de Planta",
			"Vendedor"
		];

		public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
		{
			foreach (var rol in RolesBase)
			{
				if (!await roleManager.RoleExistsAsync(rol))
				{
					await roleManager.CreateAsync(new IdentityRole(rol));
				}
			}
		}

		public static async Task SeedUsuarioAdministradorAsync(
			UserManager<UsuarioIdentity> userManager,
			IConfiguration configuracion)
		{
			var correoAdmin = configuracion["IdentitySeed:AdminEmail"] ?? "admin@PROmaderas.local";
			var contrasenaAdmin = configuracion["IdentitySeed:AdminPassword"] ?? "Admin123!";
			var nombreAdmin = configuracion["IdentitySeed:AdminNombre"] ?? "Administrador PROmaderas";

			var usuario = await userManager.FindByEmailAsync(correoAdmin);
			if (usuario == null)
			{
				usuario = new UsuarioIdentity
				{
					UserName = correoAdmin,
					Email = correoAdmin,
					EmailConfirmed = true,
					NombreCompleto = nombreAdmin
				};

				var resultadoCreacion = await userManager.CreateAsync(usuario, contrasenaAdmin);
				if (!resultadoCreacion.Succeeded)
				{
					throw new InvalidOperationException(
						$"No fue posible crear el usuario administrador inicial: {string.Join(", ", resultadoCreacion.Errors.Select(e => e.Description))}");
				}
			}

			if (!await userManager.IsInRoleAsync(usuario, "Administrador"))
			{
				await userManager.AddToRoleAsync(usuario, "Administrador");
			}
		}
	}
}
