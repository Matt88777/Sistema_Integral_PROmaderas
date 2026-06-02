using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PROmaderas.Abstracciones.Models;

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
			"Vendedor",
             "Usuario"
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
        public static async Task SeedUsuarioVendedorAsync(
    UserManager<UsuarioIdentity> userManager,
    IConfiguration configuracion)
        {
            var correo = "vendedor@PROmaderas.local";
            var password = "Vendedor123!";

            var usuario = await userManager.FindByEmailAsync(correo);

            if (usuario == null)
            {
                usuario = new UsuarioIdentity
                {
                    UserName = correo,
                    Email = correo,
                    EmailConfirmed = true,
                    NombreCompleto = "Vendedor Sistema"
                };

                var result = await userManager.CreateAsync(usuario, password);

                if (!result.Succeeded)
                    throw new Exception("Error creando vendedor");
            }

            if (!await userManager.IsInRoleAsync(usuario, "Vendedor"))
            {
                await userManager.AddToRoleAsync(usuario, "Vendedor");
            }
        }
	public static async Task SeedUsuarioGerenteAsync(
            UserManager<UsuarioIdentity> userManager)
        {
            var correo = "gerente@PROmaderas.local";
            var password = "Gerente123!";

            var usuario = await userManager.FindByEmailAsync(correo);
            if (usuario == null)
            {
                usuario = new UsuarioIdentity
                {
                    UserName = correo,
                    Email = correo,
                    EmailConfirmed = true,
                    NombreCompleto = "Gerente Sistema"
                };
                var result = await userManager.CreateAsync(usuario, password);
                if (!result.Succeeded)
                    throw new Exception("Error creando gerente: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            if (!await userManager.IsInRoleAsync(usuario, "Gerente"))
                await userManager.AddToRoleAsync(usuario, "Gerente");
        }

        public static async Task SeedUsuarioContadorAsync(
            UserManager<UsuarioIdentity> userManager)
        {
            var correo = "contador@PROmaderas.local";
            var password = "Contador123!";

            var usuario = await userManager.FindByEmailAsync(correo);
            if (usuario == null)
            {
                usuario = new UsuarioIdentity
                {
                    UserName = correo,
                    Email = correo,
                    EmailConfirmed = true,
                    NombreCompleto = "Contador Sistema"
                };
                var result = await userManager.CreateAsync(usuario, password);
                if (!result.Succeeded)
                    throw new Exception("Error creando contador: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            if (!await userManager.IsInRoleAsync(usuario, "Contador"))
                await userManager.AddToRoleAsync(usuario, "Contador");
        }

        public static async Task SeedUsuarioOperadorAsync(
            UserManager<UsuarioIdentity> userManager)
        {
            var correo = "operador@PROmaderas.local";
            var password = "Operador123!";

            var usuario = await userManager.FindByEmailAsync(correo);
            if (usuario == null)
            {
                usuario = new UsuarioIdentity
                {
                    UserName = correo,
                    Email = correo,
                    EmailConfirmed = true,
                    NombreCompleto = "Operador de Planta"
                };
                var result = await userManager.CreateAsync(usuario, password);
                if (!result.Succeeded)
                    throw new Exception("Error creando operador: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            if (!await userManager.IsInRoleAsync(usuario, "Operador de Planta"))
                await userManager.AddToRoleAsync(usuario, "Operador de Planta");
        }
public static async Task SeedUsuarioGenericoAsync(
UserManager<UsuarioIdentity> userManager)
        {
            var correo = "usuario@promaderas.local";
            var password = "Usuario123!";

            var usuario = await userManager.FindByEmailAsync(correo);
            if (usuario == null)
            {
                usuario = new UsuarioIdentity
                {
                    UserName = correo,
                    Email = correo,
                    EmailConfirmed = true,
                    NombreCompleto = "Usuario"
                };
                var result = await userManager.CreateAsync(usuario, password);
                if (!result.Succeeded)
                    throw new Exception("Error creando usuario generico: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            if (!await userManager.IsInRoleAsync(usuario, "Usuario"))
                await userManager.AddToRoleAsync(usuario, "Usuario");
        }

        public static async Task SeedUsuarioDanielaAsync(
        UserManager<UsuarioIdentity> userManager)
        {
            var correo = "dmurillo@promadera.local";
            var password = "Dmurillo123!";

            var usuario = await userManager.FindByEmailAsync(correo);
            if (usuario == null)
            {
                usuario = new UsuarioIdentity
                {
                    UserName = correo,
                    Email = correo,
                    EmailConfirmed = true,
                    NombreCompleto = "Daniela Murillo"
                };
                var result = await userManager.CreateAsync(usuario, password);
                if (!result.Succeeded)
                    throw new Exception("Error creando usuario Daniela: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            if (!await userManager.IsInRoleAsync(usuario, "Usuario"))
                await userManager.AddToRoleAsync(usuario, "Usuario");
        }

        public static async Task SeedUsuarioAborbonAsync(
        UserManager<UsuarioIdentity> userManager)
        {
            var correo = "testeo@gmail.com";
            var password = "Testeo123!";

            var usuario = await userManager.FindByEmailAsync(correo);
            if (usuario == null)
            {
                usuario = new UsuarioIdentity
                {
                    UserName = correo,
                    Email = correo,
                    EmailConfirmed = true,
                    NombreCompleto = "aborbon"
                };
                var result = await userManager.CreateAsync(usuario, password);
                if (!result.Succeeded)
                    throw new Exception("Error creando usuario aborbon: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            if (!await userManager.IsInRoleAsync(usuario, "Usuario"))
                await userManager.AddToRoleAsync(usuario, "Usuario");
        }
    }
}


