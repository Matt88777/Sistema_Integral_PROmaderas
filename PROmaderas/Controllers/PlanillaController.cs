using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROmaderas.Abstracciones.Models;
using PROmaderas.AccesoADatos;

namespace PROmaderas.UI.Controllers
{
	// Sprint 0 PROMADERAS: la BD nueva usa un modelo relacional
	// (PlanillaPeriodo + PlanillaDetalle + ...) que el modelo plano PlanillaAD
	// no soporta. Todo el módulo queda en construcción hasta que se mapee
	// correctamente en un sprint posterior. El controller original se conserva
	// abajo, comentado, para retomarlo más adelante.
	[Authorize]
	public class PlanillaController : Controller
	{
		private readonly Contexto _contexto;

		public PlanillaController(Contexto contexto)
		{
			_contexto = contexto;
			_ = _contexto; // mantener la inyección por consistencia
		}

		private IActionResult EnConstruccion()
		{
			ViewBag.Modulo = "El módulo de Planilla";
			ViewBag.Detalle = "PROMADERAS gestiona la planilla con un modelo relacional (períodos + detalle por empleado) que aún no está conectado al frontend. Estará disponible en el próximo sprint.";
			return View("EnConstruccion");
		}

		public IActionResult Index() => EnConstruccion();
		public IActionResult Details(int? id) => EnConstruccion();
		public IActionResult Create() => EnConstruccion();
		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Create(PlanillaAD planilla) => EnConstruccion();
		public IActionResult Edit(int? id) => EnConstruccion();
		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Edit(int id, PlanillaAD planilla) => EnConstruccion();
		public IActionResult Delete(int? id) => EnConstruccion();
		[HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id) => EnConstruccion();

		/* IMPLEMENTACION ORIGINAL (Pedidos360) — comentada hasta que se mapee a
		   PlanillaPeriodo + PlanillaDetalle de la BD nueva.

		public async Task<IActionResult> Index()
		{
			var lista = await _contexto.Planillas.ToListAsync();
			return View(lista);
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var planilla = await _contexto.Planillas.FirstOrDefaultAsync(p => p.Id == id);
			if (planilla == null) return NotFound();

			return View(planilla);
		}

		// ... el resto del CRUD original.
		*/
	}
}
