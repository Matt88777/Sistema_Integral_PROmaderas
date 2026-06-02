using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PROmaderas.UI.Controllers
{
    /// <summary>
    /// La gestión de usuarios se hace desde AdminUsuariosController.
    /// Este controlador redirige allá para compatibilidad de rutas.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        public IActionResult Index()
            => RedirectToAction("Index", "AdminUsuarios");
    }
}
