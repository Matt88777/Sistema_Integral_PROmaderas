using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.UI.Models;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
	[Authorize(
		Roles = Roles.Administrador + "," +
				Roles.Contador)]
	public class PolizasINSController : Controller
	{
		private readonly IPolizaINSLogica _polizaLogica;
		private readonly IEmpleadoLogica _empleadoLogica;

		public PolizasINSController(
			IPolizaINSLogica polizaLogica,
			IEmpleadoLogica empleadoLogica)
		{
			_polizaLogica = polizaLogica;
			_empleadoLogica = empleadoLogica;
		}

		public async Task<IActionResult> Index()
		{
			ViewBag.PolizasProximasAVencer =
				await _polizaLogica
					.ObtenerProximasAVencer(30);

			return View(
				await _polizaLogica.ObtenerTodas());
		}

		public async Task<IActionResult> Create()
		{
			RegistrarPolizaINSViewModel vm = new();
			await CargarEmpleados(vm);
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			RegistrarPolizaINSViewModel vm)
		{
			if (vm.FechaVencimiento <
				vm.FechaInicio)
			{
				ModelState.AddModelError(
					nameof(vm.FechaVencimiento),
					"La fecha de vencimiento no puede ser anterior al inicio.");
			}

			if (vm.IdsEmpleados == null ||
				vm.IdsEmpleados.Count == 0)
			{
				ModelState.AddModelError(
					nameof(vm.IdsEmpleados),
					"Debe seleccionar al menos un empleado.");
			}

			if (!ModelState.IsValid)
			{
				await CargarEmpleados(vm);
				return View(vm);
			}

			try
			{
				PolizaINSAD poliza = new()
				{
					NumeroPoliza = vm.NumeroPoliza,
					TipoPoliza = vm.TipoPoliza,
					Aseguradora = vm.Aseguradora,
					FechaInicio = vm.FechaInicio,
					FechaVencimiento =
						vm.FechaVencimiento,
					MontoAsegurado =
						vm.MontoAsegurado,
					Prima = vm.Prima,
					Observacion = vm.Observacion
				};

				await _polizaLogica.Registrar(
	poliza,
	vm.IdsEmpleados ?? new List<int>());

				TempData["SuccessMessage"] =
					"La póliza se registró y se asignó a los empleados seleccionados.";

				return RedirectToAction(
					nameof(Index));
			}
			catch (ArgumentException ex)
			{
				ModelState.AddModelError(
					string.Empty,
					ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError(
					string.Empty,
					ex.Message);
			}

			await CargarEmpleados(vm);
			return View(vm);
		}

		public async Task<IActionResult> Historial(
			int idEmpleado)
		{
			if (idEmpleado <= 0)
				return BadRequest();

			List<PolizaINSAD> polizas =
				await _polizaLogica
					.ObtenerHistorialEmpleado(
						idEmpleado);

			if (polizas.Count == 0)
			{
				TempData["ErrorMessage"] =
					"El empleado no tiene pólizas asignadas.";

				return RedirectToAction(
					nameof(Index));
			}

			ViewBag.IdEmpleado = idEmpleado;

			return View(polizas);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Desactivar(
			int idPoliza)
		{
			try
			{
				await _polizaLogica.Desactivar(
					idPoliza);

				TempData["SuccessMessage"] =
					"La póliza fue desactivada correctamente.";
			}
			catch (ArgumentException ex)
			{
				TempData["ErrorMessage"] =
					ex.Message;
			}
			catch (InvalidOperationException ex)
			{
				TempData["ErrorMessage"] =
					ex.Message;
			}

			return RedirectToAction(nameof(Index));
		}

		private async Task CargarEmpleados(
			RegistrarPolizaINSViewModel vm)
		{
			List<EmpleadoAD> empleados =
				await _empleadoLogica.ObtenerTodos();

			vm.IdsEmpleados ??= new List<int>();

			vm.Empleados = empleados
				.Where(e =>
					e.IdEmpleado.HasValue &&
					e.Estado == true)
				.OrderBy(e => e.Nombre)
				.ThenBy(e => e.PrimerApellido)
				.Select(e => new SelectListItem
				{
					Value =
						e.IdEmpleado!.Value.ToString(),

					Text =
						ConstruirNombreEmpleado(e),

					Selected =
						vm.IdsEmpleados.Contains(
							e.IdEmpleado.Value)
				})
				.ToList();
		}

		private static string ConstruirNombreEmpleado(
			EmpleadoAD empleado)
		{
			string nombre = string.Join(
				" ",
				new[]
				{
					empleado.Nombre,
					empleado.PrimerApellido,
					empleado.SegundoApellido
				}
				.Where(valor =>
					!string.IsNullOrWhiteSpace(
						valor)));

			if (!string.IsNullOrWhiteSpace(
				empleado.Cedula))
			{
				nombre += $" - {empleado.Cedula}";
			}

			return nombre;
		}
	}
}