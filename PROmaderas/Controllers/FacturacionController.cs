using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos;

namespace PROmaderas.UI.Controllers
{
	// Sprint 0 PROMADERAS: la BD nueva exige NumeroFactura (UNIQUE), IdUsuarioEmisor
	// y SaldoPendiente como NOT NULL — el modelo FacturacionAD no los tiene.
	// Index/Details siguen funcionando (solo lectura). Create/Edit/Delete quedan
	// en construcción hasta que se mapee la generación de NumeroFactura y se
	// conecte la lógica de usuario emisor / pagos.
	[Authorize]
	public class FacturacionController : Controller
	{
		private readonly Contexto _contexto;

		public FacturacionController(Contexto contexto)
		{
			_contexto = contexto;
		}

		public async Task<IActionResult> Index()
		{
			var lista = await _contexto.Facturaciones
				.Include(f => f.Cliente)
				.Include(f => f.Pedido)
				.ToListAsync();

			return View(lista);
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var factura = await _contexto.Facturaciones
				.Include(f => f.Cliente)
				.Include(f => f.Pedido)
				.FirstOrDefaultAsync(f => f.Id == id);

			if (factura == null) return NotFound();

			return View(factura);
		}

		private IActionResult EnConstruccion()
		{
			ViewBag.Modulo = "La creación, edición y eliminación de facturas";
			ViewBag.Detalle = "PROMADERAS exige NumeroFactura, IdUsuarioEmisor y SaldoPendiente que aún no están conectados al modelo. Estará disponible en el próximo sprint.";
			return View("EnConstruccion");
		}

		public IActionResult Create() => EnConstruccion();

		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Create(FacturacionAD factura) => EnConstruccion();

		public IActionResult Edit(int? id) => EnConstruccion();

		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Edit(int id, FacturacionAD factura) => EnConstruccion();

		public IActionResult Delete(int? id) => EnConstruccion();

		[HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id) => EnConstruccion();
	}
}
