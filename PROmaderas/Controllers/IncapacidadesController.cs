using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using PROmaderas.UI.Models;
using PROmaderas.UI.Seguridad;

namespace PROmaderas.UI.Controllers
{
	[Authorize(Roles = Roles.Administrador + "," + Roles.Contador)]
	public class IncapacidadesController : Controller
	{
		private readonly IIncapacidadLogica _incapacidadLogica;
		private readonly IEmpleadoLogica _empleadoLogica;

		public IncapacidadesController(
			IIncapacidadLogica incapacidadLogica,
			IEmpleadoLogica empleadoLogica)
		{
			_incapacidadLogica = incapacidadLogica;
			_empleadoLogica = empleadoLogica;
		}

		public async Task<IActionResult> Index()
		{
			List<IncapacidadAD> incapacidades =
				await _incapacidadLogica.ObtenerTodas();

			return View(incapacidades);
		}

		public async Task<IActionResult> Create()
		{
			RegistrarIncapacidadViewModel vm = new()
			{
				FechaInicio = DateTime.Today,
				FechaFin = DateTime.Today
			};

			await CargarListas(vm);

			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			RegistrarIncapacidadViewModel vm)
		{
			if (vm.FechaFin < vm.FechaInicio)
			{
				ModelState.AddModelError(
					nameof(vm.FechaFin),
					"La fecha de finalización no puede ser anterior a la fecha de inicio.");
			}

			if (!ModelState.IsValid)
			{
				await CargarListas(vm);
				return View(vm);
			}

			try
			{
				IncapacidadAD incapacidad = new()
				{
					IdEmpleado = vm.IdEmpleado,
					TipoIncapacidad = vm.TipoIncapacidad,
					FechaInicio = vm.FechaInicio,
					FechaFin = vm.FechaFin,
					NumeroCertificado = vm.NumeroCertificado,
					EntidadEmisora = vm.EntidadEmisora,
					Observacion = vm.Observacion
				};

				await _incapacidadLogica.Registrar(
					incapacidad);

				TempData["SuccessMessage"] =
					$"La incapacidad fue registrada correctamente por " +
					$"{incapacidad.CantidadDias} día(s).";

				return RedirectToAction(nameof(Index));
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

			await CargarListas(vm);
			return View(vm);
		}

		public async Task<IActionResult> Historial(
			int idEmpleado)
		{
			if (idEmpleado <= 0)
				return BadRequest();

			List<IncapacidadAD> incapacidades =
				await _incapacidadLogica
					.ObtenerHistorialEmpleado(idEmpleado);

			if (incapacidades.Count == 0)
			{
				TempData["ErrorMessage"] =
					"El empleado no tiene incapacidades registradas.";

				return RedirectToAction(nameof(Index));
			}

			return View(incapacidades);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Anular(
			int idIncapacidad)
		{
			try
			{
				await _incapacidadLogica.Anular(
					idIncapacidad);

				TempData["SuccessMessage"] =
					"La incapacidad fue anulada correctamente.";
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

		private async Task CargarListas(
			RegistrarIncapacidadViewModel vm)
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

			vm.TiposIncapacidad =
				TiposIncapacidad.Todos
					.Select(tipo => new SelectListItem
					{
						Value = tipo,
						Text = tipo,
						Selected =
							tipo == vm.TipoIncapacidad
					})
					.ToList();

			string[] entidades =
			{
				"CCSS",
				"INS",
				"Otra"
			};

			vm.EntidadesEmisoras = entidades
				.Select(entidad => new SelectListItem
				{
					Value = entidad,
					Text = entidad,
					Selected =
						entidad == vm.EntidadEmisora
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

			if (!string.IsNullOrWhiteSpace(
				empleado.Cedula))
			{
				nombreCompleto +=
					$" - {empleado.Cedula}";
			}

			return nombreCompleto;
		}
	}
}