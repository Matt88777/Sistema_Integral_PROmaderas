using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
	[Authorize]
	public class DashboardController : Controller
	{
		private readonly IDashboardLogica _dashboardLogica;

		public DashboardController(IDashboardLogica dashboardLogica)
		{
			_dashboardLogica = dashboardLogica;
		}

		public async Task<IActionResult> Index()
		{
			var puedeVerFinanzas =
				User.IsInRole(Roles.Administrador) ||
				User.IsInRole(Roles.Gerente) ||
				User.IsInRole(Roles.Contador);

			ViewBag.PuedeVerFinanzas = puedeVerFinanzas;

			if (!puedeVerFinanzas)
			{
				return View(new DashboardFinancieroDTO());
			}

			var dashboard = await _dashboardLogica.ObtenerDashboardFinancieroAsync();

			return View(dashboard);
		}
	}
}