using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.AccesoADatos.Seguridad;

namespace PROmaderas.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<UsuarioIdentity> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuariosController(
            UserManager<UsuarioIdentity> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearVendedor(string email, string password, string nombreCompleto)
        {
            var usuario = new UsuarioIdentity
            {
                UserName = email,
                Email = email,
                NombreCompleto = nombreCompleto
            };

            var resultado = await _userManager.CreateAsync(usuario, password);

            if (resultado.Succeeded)
            {
                await _userManager.AddToRoleAsync(usuario, "Vendedor");
                return Ok("Vendedor creado correctamente");
            }

            return BadRequest(resultado.Errors);
        }




    }
}