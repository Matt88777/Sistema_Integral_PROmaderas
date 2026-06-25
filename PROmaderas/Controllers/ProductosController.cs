using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.UI.Models;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
    [Authorize(Roles = Roles.Administrador + "," + Roles.Gerente + "," + Roles.OperadorDePlanta)]
    public class ProductosController : Controller
    {
        private readonly IProductoLogica _productoLogica;
        private readonly IWebHostEnvironment _env;

        public ProductosController(IProductoLogica productoLogica, IWebHostEnvironment env)
        {
            _productoLogica = productoLogica;
            _env = env;
        }

        private async Task<List<CategoriaAD>> ObtenerCategoriasParaEdicion(int categoriaIdActual)
        {
            var categorias = await _productoLogica.ObtenerCategorias();
            if (categorias.Any(c => c.Id == categoriaIdActual))
            {
                return categorias;
            }

            var categoriaActual = await _productoLogica.ObtenerCategoriaPorId(categoriaIdActual);
            if (categoriaActual != null)
            {
                categorias.Add(categoriaActual);
                categorias = categorias.OrderBy(c => c.Nombre).ToList();
            }

            return categorias;
        }

        public async Task<IActionResult> Index(string? filtroNombre, int? categoriaId, int pagina = 1)
        {
            int registrosPorPagina = 10;

            var (productos, totalRegistros) = await _productoLogica.ObtenerPaginado(
                pagina,
                registrosPorPagina,
                filtroNombre,
                categoriaId);

            var categorias = await _productoLogica.ObtenerCategorias();

            ViewBag.Categorias = categorias;
            ViewBag.FiltroNombre = filtroNombre;
            ViewBag.CategoriaId = categoriaId;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

            return View(productos);
        }

        public async Task<IActionResult> Inventario(int? idTipoTarima)
        {
            var modelo = new InventarioConsultaViewModel
            {
                IdTipoTarima = idTipoTarima,
                TiposTarima = await _productoLogica.ObtenerTodos(),
                Existencias = await _productoLogica.ObtenerExistenciasActuales(idTipoTarima),
                Movimientos = await _productoLogica.ObtenerHistorialMovimientos(idTipoTarima),
                ConsultaRealizada = true
            };

            return View(modelo);
        }

        // public async Task<IActionResult> Inventario(int? idTipoTarima)
        //{
        //     var modelo = new InventarioConsultaViewModel
        //     {
        //         IdTipoTarima = idTipoTarima,
        //         TiposTarima = await _productoLogica.ObtenerTodos(),
        //         Existencias = await _productoLogica.ObtenerExistenciasActuales(idTipoTarima),
        //         Movimientos = await _productoLogica.ObtenerHistorialMovimientos(idTipoTarima),
        //         ConsultaRealizada = true
        //     };
        //
        //     return View(modelo);
        // }


        [Authorize(Roles = Roles.Administrador + "," + Roles.Gerente + "," + Roles.OperadorDePlanta)]
        public async Task<IActionResult> AjustesInventario()
        {
            var modelo = new AjusteInventarioViewModel
            {
                TiposTarima = await _productoLogica.ObtenerTodos(),
                Existencias = await _productoLogica.ObtenerExistenciasActuales(null),
                Ajuste = new AjusteInventarioDTO
                {
                    TipoAjuste = TiposMovimientoInventario.AjusteEntrada
                }
            };

            return View(modelo);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador + "," + Roles.Gerente + "," + Roles.OperadorDePlanta)]
        public async Task<IActionResult> AjustesInventario(AjusteInventarioViewModel modelo)
        {
            modelo.TiposTarima = await _productoLogica.ObtenerTodos();
            modelo.Existencias = await _productoLogica.ObtenerExistenciasActuales(null);

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            try
            {
                modelo.Ajuste.CorreoUsuarioRegistro = User.Identity?.Name;

                await _productoLogica.RegistrarAjusteInventario(modelo.Ajuste);

                TempData["Mensaje"] = "Ajuste de inventario registrado correctamente.";
                return RedirectToAction(nameof(Inventario), new { idTipoTarima = modelo.Ajuste.IdTipoTarima });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(modelo);
            }
        }


        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                var producto = await _productoLogica.CambiarEstadoTipoTarima(id);
                TempData["Mensaje"] = producto.Activo
                    ? "Tipo de tarima reactivado correctamente. El historial de movimientos se mantiene disponible."
                    : "Tipo de tarima inactivado correctamente. El historial de movimientos se mantiene disponible.";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var producto = await _productoLogica.ObtenerPorId(id.Value);
            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // Sprint 0 PROMADERAS: la tabla TipoTarima exige Codigo (UNIQUE), Medida y
        // otros campos NOT NULL que el modelo ProductoAD no tiene. Stock se
        // gestiona en una tabla aparte (InventarioMovimiento). Por eso
        // Create/Edit/Delete/AjustarStock quedan en construcción y se reemplazarán
        // cuando se conecte el flujo completo. Index/Details siguen funcionando.

        private IActionResult ProductoEnConstruccion()
        {
            ViewBag.Modulo = "La creación, edición, eliminación y ajuste de stock de productos";
            ViewBag.Detalle = "PROMADERAS gestiona productos en la tabla TipoTarima (Codigo, Medida, etc.) y el stock vía InventarioMovimiento. Estará disponible en el próximo sprint.";
            return View("EnConstruccion");
        }

        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Create()
        {
            return View(new TipoTarimaCrearDTO
            {
                Activo = true
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> Create(TipoTarimaCrearDTO modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            try
            {
                var producto = new ProductoAD
                {
                    Codigo = modelo.Codigo,
                    Nombre = modelo.Nombre,
                    Medida = modelo.Medida,
                    Descripcion = modelo.Descripcion,
                    Precio = modelo.PrecioUnitario,
                    StockMinimo = modelo.StockMinimo,
                    Activo = modelo.Activo,
                    FechaCreacion = DateTime.Now,

                    // Campos ignorados por el mapeo actual, pero se asignan para evitar validaciones antiguas.
                    CategoriaId = 0,
                    ImpuestoPorc = 0,
                    Stock = 0,
                    ImagenUrl = "-"
                };

                await _productoLogica.Crear(producto);

                TempData["Mensaje"] = "Tipo de tarima registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(modelo);
            }
        }

        public IActionResult Edit(int? id) => ProductoEnConstruccion();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ProductoAD producto, IFormFile? imagen) => ProductoEnConstruccion();

        public IActionResult Delete(int? id) => ProductoEnConstruccion();

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id) => ProductoEnConstruccion();

        [Authorize(Roles = Roles.Administrador + "," + Roles.OperadorDePlanta)]
        public IActionResult AjustarStock(int? id) => ProductoEnConstruccion();

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador + "," + Roles.OperadorDePlanta)]
        public IActionResult AjustarStock(int id, int cantidad) => ProductoEnConstruccion();
    }
}