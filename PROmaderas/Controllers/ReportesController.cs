using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = Roles.Administrador + "," + Roles.Gerente + "," + Roles.Contador)]
    public class ReportesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}