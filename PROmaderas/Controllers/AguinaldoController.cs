using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
    public class AguinaldoController : Controller
    {
        private readonly IAguinaldoLogica _aguinaldoLogica;

        public AguinaldoController(IAguinaldoLogica aguinaldoLogica)
        {
            _aguinaldoLogica = aguinaldoLogica;
        }

        public async Task<IActionResult> Index(int? anio)
        {
            var anioSeleccionado = anio ?? DateTime.Now.Year;
            var resultados = await _aguinaldoLogica.CalcularPorAnio(anioSeleccionado);

            ViewBag.Anio = anioSeleccionado;
            return View(resultados);
        }
    }
}