using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
        private readonly ApplicationDbContext _identityContext;

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

        // ── LOGIN ──────────────────────────────────────────────────────────
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewBag.ReturnUrl = returnUrl;
            return View();
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
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
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
                ModelState.AddModelError("", "Se requiere autenticación de dos factores.");
                return View(model);
            }
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Falló el inicio de sesión.");
                return View(model);
            }

            // Todos los roles redirigen al Dashboard
            return RedirectToAction("Index", "Dashboard");
        }

        // ── REGISTRO ───────────────────────────────────────────────────────
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View(new RegisterViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existeUsuario = await _userManager.FindByEmailAsync(model.Email);
            if (existeUsuario != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Ya existe un usuario con este correo.");
                return View(model);
            }

            var usuario = new UsuarioIdentity
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                NombreCompleto = model.Nombre,
                PhoneNumber = model.Telefono
            };

            var resultado = await _userManager.CreateAsync(usuario, model.Password);
            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(usuario, "Usuario");
            await _signInManager.SignInAsync(usuario, isPersistent: false);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        // ── SETUP PASSWORD (invitación interna) ───────────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> SetupPassword(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                TempData["Error"] = "El enlace de activación no es válido.";
                return RedirectToAction(nameof(Login));
            }

            var usuario = await _userManager.FindByEmailAsync(email);
            if (usuario is null)
            {
                TempData["Error"] = "No se encontró la cuenta asociada al enlace.";
                return RedirectToAction(nameof(Login));
            }

            return View(new SetupPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetupPassword(SetupPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _userManager.FindByEmailAsync(model.Email);
            if (usuario is null)
            {
                ModelState.AddModelError(string.Empty, "No se encontró la cuenta.");
                return View(model);
            }

            string tokenDecodificado;
            try
            {
                tokenDecodificado = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "El enlace no es válido o expiró.");
                return View(model);
            }

            var resultado = await _userManager.ResetPasswordAsync(usuario, tokenDecodificado, model.Password);
            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            TempData["Mensaje"] = "Contraseña configurada. Ya puedes iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }

        // ── FORGOT / RESET PASSWORD ────────────────────────────────────────
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

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
                HtmlBody = $"<p>Haz clic aquí para restablecer tu contraseña:</p><p><a href='{resetUrl}'>{resetUrl}</a></p>",
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
                TempData["Error"] = "El enlace no es válido.";
                return RedirectToAction(nameof(Login));
            }
            return View(new SetupPasswordViewModel { Email = email, Token = token });
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
                ModelState.AddModelError(string.Empty, "No se encontró la cuenta.");
                return View(model);
            }

            string tokenDecodificado;
            try
            {
                tokenDecodificado = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "El enlace no es válido o expiró.");
                return View(model);
            }

            var resultado = await _userManager.ResetPasswordAsync(usuario, tokenDecodificado, model.Password);
            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            TempData["Mensaje"] = "Contraseña restablecida. Ya puedes iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }

        // ── LOGOUT ─────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();

        // ── MI PERFIL — todos los roles ────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(Login));

            var roles = await _userManager.GetRolesAsync(user);

            return View(new PerfilViewModel
            {
                Email = user.Email ?? "",
                NombreCompleto = user.NombreCompleto ?? "",
                Telefono = user.PhoneNumber ?? "",
                Rol = roles.FirstOrDefault() ?? "Sin rol"
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(PerfilViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(Login));

            user.NombreCompleto = model.NombreCompleto;
            user.PhoneNumber = model.Telefono;

            await _userManager.UpdateAsync(user);

            TempData["Mensaje"] = "Perfil actualizado correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        // ── solo desarrollo)
        [AllowAnonymous]
        public async Task<IActionResult> ResetAdminPrueba()
        {
            var totalIdentity = _userManager.Users.Count();
            var conexion = _identityContext.Database.GetDbConnection().ConnectionString;
            return Content($"Total usuarios: {totalIdentity}\nConexion: {conexion}");
        }
    }
}
