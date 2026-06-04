using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using PROmaderas.AccesoADatos.Seguridad;
using PROmaderas.UI.Models;
using PROmaderas.UI.Services;
using System.Security.Claims;

namespace PROmaderas.UI.Controllers;

[Authorize(Roles = "Administrador")]
public class AdminUsuariosController : Controller
{
	private static readonly string[] RolesInternos =
	[
		"Administrador",
		"Gerente",
		"Contador",
		"Operador de Planta",
		"Vendedor"
	];

	private readonly UserManager<UsuarioIdentity> _userManager;
	private readonly IMailStore _mailStore;

	public AdminUsuariosController(UserManager<UsuarioIdentity> userManager, IMailStore mailStore)
	{
		_userManager = userManager;
		_mailStore = mailStore;
	}

	public async Task<IActionResult> Index()
	{
		var usuarios = await _userManager.Users
			.OrderBy(x => x.NombreCompleto)
			.ThenBy(x => x.Email)
			.ToListAsync();

		var resultado = new List<AdminUserListItemViewModel>();

		foreach (var usuario in usuarios)
		{
			var roles = await _userManager.GetRolesAsync(usuario);
			var rolInterno = roles.FirstOrDefault(r => RolesInternos.Contains(r));

			if (rolInterno is null)
			{
				continue;
			}

			resultado.Add(new AdminUserListItemViewModel
			{
				Id = usuario.Id,
				Nombre = usuario.NombreCompleto,
				Email = usuario.Email ?? string.Empty,
				Telefono = usuario.PhoneNumber,
				Rol = rolInterno,
				TieneContrasena = !string.IsNullOrWhiteSpace(usuario.PasswordHash),
				UltimaInvitacionUtc = usuario.Email is null
					? null
					: _mailStore.GetLatestForRecipient(usuario.Email)?.CreatedAtUtc
			});
		}

		return View(resultado);
	}

	public IActionResult Create()
	{
		return View(new AdminCreateUserViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(AdminCreateUserViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View(model);
		}

		if (!RolesInternos.Contains(model.Rol))
		{
			ModelState.AddModelError(
				nameof(model.Rol),
				"Solo se pueden crear usuarios con un rol permitido (Administrador, Gerente, Contador, Operador de Planta o Vendedor)."
			);

			return View(model);
		}

		var existente = await _userManager.FindByEmailAsync(model.Email);

		if (existente is not null)
		{
			ModelState.AddModelError(nameof(model.Email), "Ya existe un usuario registrado con este correo.");
			return View(model);
		}

		var usuario = new UsuarioIdentity
		{
			UserName = model.Email,
			Email = model.Email,
			PhoneNumber = model.Telefono,
			NombreCompleto = model.Nombre,
			EmailConfirmed = true
		};

		var resultadoCreacion = await _userManager.CreateAsync(usuario);

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

		var mail = await CrearCorreoInvitacionAsync(usuario);

		TempData["Mensaje"] = $"Usuario con rol '{model.Rol}' creado. Se genero el correo para definir contrasena.";

		return RedirectToAction(nameof(EmailPreview), new { id = mail.Id });
	}

	[HttpGet]
	public async Task<IActionResult> EditarRol(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			TempData["Error"] = "Debe indicar un usuario.";
			return RedirectToAction(nameof(Index));
		}

		var usuario = await _userManager.FindByIdAsync(id);

		if (usuario is null)
		{
			TempData["Error"] = "No se encontro el usuario indicado.";
			return RedirectToAction(nameof(Index));
		}

		var roles = await _userManager.GetRolesAsync(usuario);
		var rolActual = roles.FirstOrDefault(r => RolesInternos.Contains(r)) ?? "Vendedor";

		return View(new AdminEditRoleViewModel
		{
			UserId = usuario.Id,
			Nombre = usuario.NombreCompleto,
			Email = usuario.Email ?? string.Empty,
			Rol = rolActual
		});
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> EditarRol(AdminEditRoleViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View(model);
		}

		if (!RolesInternos.Contains(model.Rol))
		{
			ModelState.AddModelError(nameof(model.Rol), "Seleccione un rol valido.");
			return View(model);
		}

		var usuario = await _userManager.FindByIdAsync(model.UserId);

		if (usuario is null)
		{
			TempData["Error"] = "No se encontro el usuario indicado.";
			return RedirectToAction(nameof(Index));
		}

		var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (usuario.Id == usuarioActualId && model.Rol != "Administrador")
		{
			ModelState.AddModelError(
				nameof(model.Rol),
				"No puede quitarse a usted mismo el rol Administrador."
			);

			model.Nombre = usuario.NombreCompleto;
			model.Email = usuario.Email ?? string.Empty;

			return View(model);
		}

		var rolesActuales = await _userManager.GetRolesAsync(usuario);
		var rolesGestionados = rolesActuales
			.Where(r => RolesInternos.Contains(r))
			.ToList();

		if (rolesGestionados.Any())
		{
			var quitarRoles = await _userManager.RemoveFromRolesAsync(usuario, rolesGestionados);

			if (!quitarRoles.Succeeded)
			{
				foreach (var error in quitarRoles.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				model.Nombre = usuario.NombreCompleto;
				model.Email = usuario.Email ?? string.Empty;

				return View(model);
			}
		}

		var agregarRol = await _userManager.AddToRoleAsync(usuario, model.Rol);

		if (!agregarRol.Succeeded)
		{
			foreach (var error in agregarRol.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			model.Nombre = usuario.NombreCompleto;
			model.Email = usuario.Email ?? string.Empty;

			return View(model);
		}

		TempData["Mensaje"] = $"Rol actualizado correctamente para {usuario.Email}.";

		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ReenviarInvitacion(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
		{
			TempData["Error"] = "Debe indicar un correo valido.";
			return RedirectToAction(nameof(Index));
		}

		var usuario = await _userManager.FindByEmailAsync(email);

		if (usuario is null)
		{
			TempData["Error"] = "No se encontro el usuario indicado.";
			return RedirectToAction(nameof(Index));
		}

		var roles = await _userManager.GetRolesAsync(usuario);

		if (!roles.Any(r => RolesInternos.Contains(r)))
		{
			TempData["Error"] = "Solo se pueden reenviar invitaciones a usuarios internos.";
			return RedirectToAction(nameof(Index));
		}

		var mail = await CrearCorreoInvitacionAsync(usuario);

		TempData["Mensaje"] = "Invitacion reenviada a la bandeja de correo.";

		return RedirectToAction(nameof(EmailPreview), new { id = mail.Id });
	}

	[HttpGet]
	public async Task<IActionResult> RestablecerContrasena(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			TempData["Error"] = "Debe indicar un usuario.";
			return RedirectToAction(nameof(Index));
		}

		var usuario = await _userManager.FindByIdAsync(id);

		if (usuario is null)
		{
			TempData["Error"] = "No se encontro el usuario indicado.";
			return RedirectToAction(nameof(Index));
		}

		return View(new AdminResetPasswordViewModel
		{
			UserId = usuario.Id,
			Email = usuario.Email ?? string.Empty
		});
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> RestablecerContrasena(AdminResetPasswordViewModel modelo)
	{
		if (!ModelState.IsValid)
		{
			return View(modelo);
		}

		var usuario = await _userManager.FindByIdAsync(modelo.UserId);

		if (usuario is null)
		{
			TempData["Error"] = "No se encontro el usuario indicado.";
			return RedirectToAction(nameof(Index));
		}

		var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
		var resultado = await _userManager.ResetPasswordAsync(usuario, token, modelo.NuevaContrasena);

		if (resultado.Succeeded)
		{
			TempData["Mensaje"] = $"Contrasena restablecida para {usuario.Email}.";
			return RedirectToAction(nameof(Index));
		}

		foreach (var error in resultado.Errors)
		{
			ModelState.AddModelError(string.Empty, error.Description);
		}

		modelo.Email = usuario.Email ?? string.Empty;

		return View(modelo);
	}

	public IActionResult Inbox()
	{
		return View(_mailStore.GetAll());
	}

	public IActionResult EmailPreview(int id)
	{
		var mail = _mailStore.GetById(id);

		if (mail is null)
		{
			TempData["Error"] = "El correo solicitado no existe.";
			return RedirectToAction(nameof(Inbox));
		}

		return View(mail);
	}

	private async Task<MailMessage> CrearCorreoInvitacionAsync(UsuarioIdentity usuario)
	{
		var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
		var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

		var actionUrl = Url.Action(
			action: "SetupPassword",
			controller: "Account",
			values: new { email = usuario.Email, token = tokenEncoded },
			protocol: Request.Scheme) ?? string.Empty;

		var html = $"""
        <div style='font-family:Segoe UI, Arial, sans-serif; max-width:680px; margin:0 auto; padding:24px; background:#f7f8fa;'>
            <div style='background:#ffffff; border:1px solid #d9e1ea; border-radius:16px; overflow:hidden;'>
                <div style='padding:24px; background:#0f766e; color:#ffffff;'>
                    <h2 style='margin:0;'>PROmaderas</h2>
                    <p style='margin:8px 0 0;'>Invitacion para activar tu acceso interno</p>
                </div>
                <div style='padding:24px; color:#1f2937;'>
                    <p>Hola {usuario.NombreCompleto},</p>
                    <p>Se te asigno un acceso interno en PROmaderas. Para activar tu cuenta y definir tu contrasena, usa el siguiente enlace:</p>
                    <p style='margin:24px 0;'>
                        <a href='{actionUrl}' style='background:#ea580c; color:#ffffff; text-decoration:none; padding:12px 18px; border-radius:10px; display:inline-block;'>Definir contrasena</a>
                    </p>
                    <p>Si el boton no funciona, copia y pega esta URL en tu navegador:</p>
                    <p style='word-break:break-all; color:#0f766e;'>{actionUrl}</p>
                    <hr style='border:none; border-top:1px solid #e5e7eb; margin:24px 0;' />
                    <p style='font-size:14px; color:#6b7280;'>Si no solicitaste este acceso, puedes ignorar este mensaje.</p>
                </div>
            </div>
        </div>
        """;

		return _mailStore.Add(new MailMessage
		{
			To = usuario.Email ?? string.Empty,
			Subject = "Activa tu acceso a PROmaderas",
			HtmlBody = html,
			ActionUrl = actionUrl
		});
	}
}