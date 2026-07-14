using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.UI.Models;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
	[Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
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
			List<PolizaINSAD> polizas =
				await _polizaLogica.ObtenerTodas();

			ViewBag.PolizasProximasAVencer =
				await _polizaLogica.ObtenerProximasAVencer(30);

			return View(polizas);
		}

		public async Task<IActionResult> Create()
		{
			RegistrarPolizaINSViewModel vm = new()
			{
				FechaInicio = DateTime.Today,
				FechaVencimiento = DateTime.Today.AddYears(1)
			};

			await CargarEmpleados(vm);

			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			RegistrarPolizaINSViewModel vm)
		{
			if (vm.FechaVencimiento < vm.FechaInicio)
			{
				ModelState.AddModelError(
					nameof(vm.FechaVencimiento),
					"La fecha de vencimiento no puede ser anterior a la fecha de inicio.");
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
					IdEmpleado = vm.IdEmpleado,
					NumeroPoliza = vm.NumeroPoliza,
					FechaInicio = vm.FechaInicio,
					FechaVencimiento = vm.FechaVencimiento,
					Cobertura = vm.Cobertura,
					Observacion = vm.Observacion
				};

				await _polizaLogica.Registrar(poliza);

				TempData["SuccessMessage"] =
					"La póliza del INS fue registrada correctamente y quedó activa.";

				return RedirectToAction(nameof(Index));
			}
			catch (ArgumentException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
			}

			await CargarEmpleados(vm);
			return View(vm);
		}

		public async Task<IActionResult> Historial(int idEmpleado)
		{
			if (idEmpleado <= 0)
				return BadRequest();

			List<PolizaINSAD> polizas =
				await _polizaLogica.ObtenerHistorialEmpleado(
					idEmpleado);

			if (polizas.Count == 0)
			{
				TempData["ErrorMessage"] =
					"El empleado no tiene pólizas registradas.";

				return RedirectToAction(nameof(Index));
			}

			ViewBag.IdEmpleado = idEmpleado;

			return View(polizas);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Desactivar(
			int idPolizaINS)
		{
			try
			{
				await _polizaLogica.Desactivar(idPolizaINS);

				TempData["SuccessMessage"] =
					"La póliza fue desactivada correctamente.";
			}
			catch (ArgumentException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (InvalidOperationException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}

			return RedirectToAction(nameof(Index));
		}

		private async Task CargarEmpleados(
			RegistrarPolizaINSViewModel vm)
		{
			List<EmpleadoAD> empleados =
				await _empleadoLogica.ObtenerTodos();

			vm.Empleados = empleados
				.Where(e =>
					e.IdEmpleado.HasValue &&
					e.Estado == true)
				.OrderBy(e => e.Nombre)
				.ThenBy(e => e.PrimerApellido)
				.Select(e => new SelectListItem
				{
					Value = e.IdEmpleado!.Value.ToString(),
					Text = ConstruirNombreEmpleado(e),
					Selected =
						e.IdEmpleado.Value == vm.IdEmpleado
				})
				.ToList();
		}

		private static string ConstruirNombreEmpleado(
			EmpleadoAD empleado)
		{
			string nombreCompleto = string.Join(
				" ",
				new[]
				{
					empleado.Nombre,
					empleado.PrimerApellido,
					empleado.SegundoApellido
				}
				.Where(valor =>
					!string.IsNullOrWhiteSpace(valor)));

			if (!string.IsNullOrWhiteSpace(empleado.Cedula))
			{
				nombreCompleto +=
					$" - {empleado.Cedula}";
			}

			return nombreCompleto;
		}
	}
}