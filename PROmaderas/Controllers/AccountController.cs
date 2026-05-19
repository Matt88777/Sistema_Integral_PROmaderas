using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos;
using PROmaderas.AccesoADatos.Seguridad;
using PROmaderas.UI.Models;
using PROmaderas.UI.Services;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PROmaderas.UI.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<UsuarioIdentity> _userManager;
		private readonly SignInManager<UsuarioIdentity> _signInManager;
		private readonly Contexto _contexto;
		private readonly IMailStore _mailStore;

		public AccountController(
			UserManager<UsuarioIdentity> userManager,
			SignInManager<UsuarioIdentity> signInManager,
			Contexto contexto,
			ApplicationDbContext identityContext,
			IMailStore mailStore)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_contexto = contexto;
			_identityContext = identityContext;
			_mailStore = mailStore;
		}

		public IActionResult Login(string? returnUrl = null)
		{
			if (User.Identity?.IsAuthenticated == true)
				return RedirectToAction("Index", "Dashboard");

			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		[AllowAnonymous]
		public IActionResult Register(string? returnUrl = null)
		{
			if (User.Identity?.IsAuthenticated == true)
				return RedirectToAction("Index", "Productos");

			return View(new RegisterViewModel { ReturnUrl = returnUrl });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
		{
			if (!ModelState.IsValid)
				return View(model);

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
				return View(model);
			}

			var passwordValida = await _userManager.CheckPasswordAsync(user, model.Password);

			if (!passwordValida)
			{
				ModelState.AddModelError("", "La contraseña no coincide con la almacenada en Identity.");
				return View(model);
			}

			var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

			if (result.IsLockedOut)
			{
				ModelState.AddModelError("", "El usuario está bloqueado.");
				return View(model);
			}

			if (result.IsNotAllowed)
			{
				ModelState.AddModelError("", "El usuario no tiene permiso para iniciar sesión.");
				return View(model);
			}

			if (result.RequiresTwoFactor)
			{
				ModelState.AddModelError("", "El usuario requiere autenticación de dos factores.");
				return View(model);
			}

			if (!result.Succeeded)
			{
				ModelState.AddModelError("", "Falló el inicio de sesión por una razón no identificada.");
				return View(model);
			}

			// Redirección por rol (roles PROMADERAS)
			var roles = await _userManager.GetRolesAsync(user);

			if (roles.Contains("Administrador") ||
				roles.Contains("Gerente") ||
				roles.Contains("Contador") ||
				roles.Contains("Vendedor") ||
				roles.Contains("Operador de Planta"))
			{
				return RedirectToAction("Index", "Dashboard");
			}

			// NOTA: el rol "Cliente" fue removido en Sprint 0; los clientes ya no
			// son usuarios del sistema. La redirección a BienvenidaCliente queda
			// inalcanzable a propósito.

			// Por defecto
			return RedirectToAction("Login");
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
		{
			// Sprint 0 PROMADERAS: el registro público está deshabilitado.
			// Los clientes no son usuarios del sistema, y los usuarios internos
			// se crean desde /AdminUsuarios. El bloque original queda comentado
			// abajo por si se retoma en un sprint posterior.
			ModelState.AddModelError(string.Empty,
				"El registro público está deshabilitado. Solicite a un administrador la creación de su cuenta.");
			return View(model);

			/*
			if (!ModelState.IsValid)
				return View(model);

			var rolesValidos = new[] { "Cliente" };
			if (!rolesValidos.Contains(model.Rol))
			{
				ModelState.AddModelError(nameof(model.Rol), "El auto-registro solo está habilitado para clientes.");
				return View(model);
			}

			var existeUsuario = await _userManager.FindByEmailAsync(model.Email);
			if (existeUsuario != null)
			{
				ModelState.AddModelError(nameof(model.Email), "Ya existe un usuario registrado con este correo.");
				return View(model);
			}

			var usuario = new UsuarioIdentity
			{
				UserName = model.Email,
				Email = model.Email,
				PhoneNumber = model.Telefono,
				NombreCompleto = model.Nombre
			};

			var resultadoCreacion = await _userManager.CreateAsync(usuario, model.Password);
			if (!resultadoCreacion.Succeeded)
			{
				foreach (var error in resultadoCreacion.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				return View(model);
			}

			var resultadoRol = await _userManager.AddToRoleAsync(usuario, model.Rol);
			if (!resultadoRol.Succeeded)
			{
				foreach (var error in resultadoRol.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				await _userManager.DeleteAsync(usuario);
				return View(model);
			}

			if (model.Rol == "Cliente")
			{
				var clienteExistente = _contexto.Clientes.FirstOrDefault(c => c.Correo == model.Email);
				if (clienteExistente == null)
				{
					_contexto.Clientes.Add(new ClienteAD
					{
						Nombre = model.Nombre,
						Cedula = model.Cedula,
						Correo = model.Email,
						Telefono = model.Telefono,
						Direccion = model.Direccion,
						UsuarioIdentityId = usuario.Id
					});
				}
				else
				{
					clienteExistente.Nombre = model.Nombre;
					clienteExistente.Cedula = model.Cedula;
					clienteExistente.Telefono = model.Telefono;
					clienteExistente.Direccion = model.Direccion;
					clienteExistente.UsuarioIdentityId = usuario.Id;
				}

				await _contexto.SaveChangesAsync();
			}

			if (User.Identity?.IsAuthenticated != true)
			{
				await _signInManager.SignInAsync(usuario, isPersistent: false);
			}

			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
				return Redirect(returnUrl);

			return RedirectToAction("BienvenidaCliente");
			*/
		}

		[AllowAnonymous]
		public async Task<IActionResult> SetupPassword(string email, string token)
		{
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
			{
				TempData["Error"] = "El enlace de activacion no es valido.";
				return RedirectToAction(nameof(Login));
			}

			var usuario = await _userManager.FindByEmailAsync(email);
			if (usuario is null)
			{
				TempData["Error"] = "No se encontro la cuenta asociada al enlace.";
				return RedirectToAction(nameof(Login));
			}

			var roles = await _userManager.GetRolesAsync(usuario);
			if (roles.Contains("Cliente"))
			{
				return RedirectToAction(nameof(AccessDenied));
			}

			return View(new SetupPasswordViewModel
			{
				Email = email,
				Token = token
			});
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SetupPassword(SetupPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var usuario = await _userManager.FindByEmailAsync(model.Email);
			if (usuario is null)
			{
				ModelState.AddModelError(string.Empty, "No se encontro la cuenta asociada al enlace.");
				return View(model);
			}

			var roles = await _userManager.GetRolesAsync(usuario);
			if (roles.Contains("Cliente"))
			{
				return RedirectToAction(nameof(AccessDenied));
			}

			string tokenDecodificado;
			try
			{
				tokenDecodificado = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
			}
			catch
			{
				ModelState.AddModelError(string.Empty, "El enlace de activacion no es valido o expiró.");
				return View(model);
			}

			var resultado = await _userManager.ResetPasswordAsync(usuario, tokenDecodificado, model.Password);
			if (!resultado.Succeeded)
			{
				foreach (var error in resultado.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				return View(model);
			}

			TempData["Mensaje"] = "Contrasena configurada correctamente. Ya puedes iniciar sesion.";
			return RedirectToAction(nameof(Login));
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Login");
		}

		public IActionResult AccessDenied()
		{
			return View();
		}

		[AllowAnonymous]
		public async Task<IActionResult> ResetAdminPrueba()
		{
			var conexionContexto = _contexto.Database.GetDbConnection().ConnectionString;
			var conexionIdentity = _identityContext.Database.GetDbConnection().ConnectionString;

			var totalIdentity = _userManager.Users.Count();
			var existeAbi = _userManager.Users.Any(u => u.Email == "abiadmin@PROmaderas.com");

			return Content(
				"Conexion Contexto: " + conexionContexto +
				"\nConexion Identity: " + conexionIdentity +
				"\nTotal usuarios Identity: " + totalIdentity +
				"\nExiste abiadmin: " + existeAbi
			);
		}

		private readonly ApplicationDbContext _identityContext;

		[AllowAnonymous]
		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var usuario = await _userManager.FindByEmailAsync(model.Email);
			if (usuario == null)
			{
				// No revelar si el correo existe o no
				TempData["Mensaje"] = "Si el correo está registrado, recibirás un enlace para restablecer tu contraseña.";
				return RedirectToAction(nameof(ForgotPassword));
			}

			var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
			var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
			var resetUrl = Url.Action(nameof(ResetPassword), "Account",
				new { email = model.Email, token = tokenEncoded }, Request.Scheme)!;

			_mailStore.Add(new MailMessage
			{
				To = model.Email,
				Subject = "Restablecer contraseña - PROmaderas",
				HtmlBody = $"<p>Haz clic en el siguiente enlace para restablecer tu contraseña:</p><p><a href='{resetUrl}'>{resetUrl}</a></p>",
				ActionUrl = resetUrl
			});

			TempData["ResetUrl"] = resetUrl;
			return RedirectToAction(nameof(ForgotPassword));
		}

		[AllowAnonymous]
		public IActionResult ResetPassword(string email, string token)
		{
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
			{
				TempData["Error"] = "El enlace de restablecimiento no es válido.";
				return RedirectToAction(nameof(Login));
			}

			return View(new SetupPasswordViewModel
			{
				Email = email,
				Token = token
			});
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(SetupPasswordViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var usuario = await _userManager.FindByEmailAsync(model.Email);
			if (usuario == null)
			{
				ModelState.AddModelError(string.Empty, "No se encontró la cuenta asociada.");
				return View(model);
			}

			string tokenDecodificado;
			try
			{
				tokenDecodificado = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
			}
			catch
			{
				ModelState.AddModelError(string.Empty, "El enlace de restablecimiento no es válido o expiró.");
				return View(model);
			}

			var resultado = await _userManager.ResetPasswordAsync(usuario, tokenDecodificado, model.Password);
			if (!resultado.Succeeded)
			{
				foreach (var error in resultado.Errors)
					ModelState.AddModelError(string.Empty, error.Description);
				return View(model);
			}

			TempData["Mensaje"] = "Contraseña restablecida correctamente. Ya puedes iniciar sesión.";
			return RedirectToAction(nameof(Login));
		}

		[Authorize(Roles = "Cliente")]
		public IActionResult BienvenidaCliente()
		{
			return View();
		}

		[Authorize(Roles = "Cliente")]
		public async Task<IActionResult> MiPerfil()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return RedirectToAction(nameof(Login));

			var cliente = await _contexto.Clientes
				.FirstOrDefaultAsync(c => c.UsuarioIdentityId == user.Id);

			if (cliente == null)
				return NotFound();

			var model = new ClientePerfilViewModel
			{
				Nombre = cliente.Nombre,
				Cedula = cliente.Cedula,
				Correo = cliente.Correo,
				Telefono = cliente.Telefono,
				Direccion = cliente.Direccion
			};

			return View(model);
		}

		[HttpPost]
		[Authorize(Roles = "Cliente")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MiPerfil(ClientePerfilViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return RedirectToAction(nameof(Login));

			var cliente = await _contexto.Clientes
				.FirstOrDefaultAsync(c => c.UsuarioIdentityId == user.Id);

			if (cliente == null)
				return NotFound();

			cliente.Nombre = model.Nombre;
			cliente.Cedula = model.Cedula;
			cliente.Correo = model.Correo;
			cliente.Telefono = model.Telefono;
			cliente.Direccion = model.Direccion;

			user.Email = model.Correo;
			user.UserName = model.Correo;
			user.NormalizedEmail = model.Correo.ToUpper();
			user.NormalizedUserName = model.Correo.ToUpper();
			user.PhoneNumber = model.Telefono;
			user.NombreCompleto = model.Nombre;

			await _contexto.SaveChangesAsync();
			await _userManager.UpdateAsync(user);

			TempData["Mensaje"] = "Perfil actualizado correctamente.";
			return RedirectToAction(nameof(MiPerfil));
		}

		[Authorize(Roles = "Cliente")]
		public async Task<IActionResult> EliminarCuenta()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return RedirectToAction(nameof(Login));

			var cliente = await _contexto.Clientes
				.FirstOrDefaultAsync(c => c.UsuarioIdentityId == user.Id);

			if (cliente == null)
				return NotFound();

			return View(cliente);
		}

		[HttpPost, ActionName("EliminarCuenta")]
		[Authorize(Roles = "Cliente")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EliminarCuentaConfirmada()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return RedirectToAction(nameof(Login));

			var cliente = await _contexto.Clientes
				.FirstOrDefaultAsync(c => c.UsuarioIdentityId == user.Id);

			if (cliente != null)
			{
				_contexto.Clientes.Remove(cliente);
				await _contexto.SaveChangesAsync();
			}

			await _signInManager.SignOutAsync();
			await _userManager.DeleteAsync(user);

			return RedirectToAction(nameof(Login));
		}

	}
}