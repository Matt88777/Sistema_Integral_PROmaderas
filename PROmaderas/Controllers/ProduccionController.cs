using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    /// <summary>
    /// INV-HU-001 — Registro de producción semanal de tarimas.
    /// Roles permitidos: Administrador, Gerente, OperadorDePlanta.
    /// </summary>
    [Authorize(Roles = Roles.Administrador + "," + Roles.Gerente + "," + Roles.OperadorDePlanta)]
    public class ProduccionController : Controller
    {
        private readonly IProduccionLogica _produccionLogica;
        private readonly IProductoLogica _productoLogica;

        public ProduccionController(
            IProduccionLogica produccionLogica,
            IProductoLogica productoLogica)
        {
            _produccionLogica = produccionLogica;
            _productoLogica = productoLogica;
        }

        // GET /Produccion
        public async Task<IActionResult> Index(int pagina = 1)
        {
            int registrosPorPagina = 10;

            var (movimientos, totalRegistros) = await _produccionLogica
                .ObtenerHistorial(pagina, registrosPorPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);
            ViewBag.TotalRegistros = totalRegistros;

            return View(movimientos);
        }

        // GET /Produccion/Registrar
        public async Task<IActionResult> Registrar()
        {
            await CargarTiposTarima();
            return View(new ProduccionSemanalDTO { FechaProduccion = DateTime.Today });
        }

        // POST /Produccion/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(ProduccionSemanalDTO modelo)
        {
            // Escenario 2: validación de anotaciones del modelo
            if (!ModelState.IsValid)
            {
                await CargarTiposTarima(modelo.IdTipoTarima);
                return View(modelo);
            }

            try
            {
                // Resolver el Id entero del operador autenticado
                var correo = User.Identity?.Name ?? string.Empty;
                var idUsuario = await _produccionLogica.ObtenerIdUsuarioPorCorreo(correo) ?? 0;

                // Escenarios 1 y 3: la lógica lanza ArgumentException ante datos inválidos
                await _produccionLogica.RegistrarProduccion(modelo, idUsuario);

                TempData["Mensaje"] = $"Producción registrada correctamente: {modelo.Cantidad} unidades el {modelo.FechaProduccion:dd/MM/yyyy}.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                // Escenarios 2/3: mostrar el mensaje de error en el formulario
                ModelState.AddModelError(string.Empty, ex.Message);
                await CargarTiposTarima(modelo.IdTipoTarima);
                return View(modelo);
            }
        }

        // Auxiliar: pobla el SelectList de tipos de tarima
        private async Task CargarTiposTarima(int seleccionado = 0)
        {
            var tipos = await _productoLogica.ObtenerTodos();
            ViewBag.TiposTarima = new SelectList(tipos, "Id", "Nombre", seleccionado);
        }
    }
}