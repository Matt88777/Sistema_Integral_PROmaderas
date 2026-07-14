using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.PolizaINS
{
	public class PolizaINSRepositorio :
		IPolizaINSRepositorio
	{
		private readonly Contexto _contexto;

		public PolizaINSRepositorio(
			Contexto contexto)
		{
			_contexto = contexto;
		}

		public async Task<List<PolizaINSAD>> ObtenerTodas()
		{
			return await _contexto.PolizasINS
				.AsNoTracking()
				.Include(p => p.EmpleadosAsignados)
					.ThenInclude(ep => ep.Empleado)
				.OrderBy(p => p.FechaVencimiento)
				.ThenBy(p => p.NumeroPoliza)
				.ToListAsync();
		}

		public async Task<PolizaINSAD?> ObtenerPorId(
			int idPoliza)
		{
			return await _contexto.PolizasINS
				.AsNoTracking()
				.Include(p => p.EmpleadosAsignados)
					.ThenInclude(ep => ep.Empleado)
				.FirstOrDefaultAsync(
					p => p.IdPoliza == idPoliza);
		}

		public async Task<List<PolizaINSAD>>
			ObtenerPorEmpleado(int idEmpleado)
		{
			return await _contexto.EmpleadosPolizas
				.AsNoTracking()
				.Where(ep =>
					ep.IdEmpleado == idEmpleado)
				.Include(ep => ep.Poliza)
					.ThenInclude(p =>
						p!.EmpleadosAsignados)
				.Select(ep => ep.Poliza!)
				.OrderByDescending(p =>
					p.FechaInicio)
				.ToListAsync();
		}

		public async Task<PolizaINSAD?>
			ObtenerPolizaVigente(
				int idEmpleado,
				DateTime fechaEvaluacion)
		{
			DateTime fecha =
				fechaEvaluacion.Date;

			return await _contexto.EmpleadosPolizas
				.AsNoTracking()
				.Where(ep =>
					ep.IdEmpleado == idEmpleado &&
					ep.Activa &&
					ep.FechaAsignacion <= fecha &&
					(!ep.FechaExclusion.HasValue ||
					 ep.FechaExclusion.Value >= fecha) &&
					ep.Poliza != null &&
					ep.Poliza.Estado &&
					ep.Poliza.FechaInicio <= fecha &&
					ep.Poliza.FechaVencimiento >= fecha)
				.Select(ep => ep.Poliza!)
				.OrderByDescending(p =>
					p.FechaVencimiento)
				.FirstOrDefaultAsync();
		}

		public async Task<List<PolizaINSAD>>
			ObtenerProximasAVencer(
				DateTime fechaDesde,
				DateTime fechaHasta)
		{
			DateTime desde = fechaDesde.Date;
			DateTime hasta = fechaHasta.Date;

			return await _contexto.PolizasINS
				.AsNoTracking()
				.Include(p => p.EmpleadosAsignados)
					.ThenInclude(ep => ep.Empleado)
				.Where(p =>
					p.Estado &&
					p.FechaVencimiento >= desde &&
					p.FechaVencimiento <= hasta)
				.OrderBy(p => p.FechaVencimiento)
				.ToListAsync();
		}

		public async Task<bool> ExisteNumeroPoliza(
			string numeroPoliza,
			int? idPolizaExcluir = null)
		{
			string numero =
				numeroPoliza.Trim();

			return await _contexto.PolizasINS
				.AsNoTracking()
				.AnyAsync(p =>
					p.NumeroPoliza == numero &&
					(!idPolizaExcluir.HasValue ||
					 p.IdPoliza !=
					 idPolizaExcluir.Value));
		}

		public async Task Crear(
			PolizaINSAD poliza,
			List<int> idsEmpleados)
		{
			await using var transaccion =
				await _contexto.Database
					.BeginTransactionAsync();

			try
			{
				_contexto.PolizasINS.Add(poliza);
				await _contexto.SaveChangesAsync();

				DateTime fechaAsignacion =
					poliza.FechaInicio.Date;

				foreach (int idEmpleado in
						 idsEmpleados.Distinct())
				{
					_contexto.EmpleadosPolizas.Add(
						new EmpleadoPolizaAD
						{
							IdPoliza =
								poliza.IdPoliza,

							IdEmpleado =
								idEmpleado,

							FechaAsignacion =
								fechaAsignacion,

							Activa = true
						});
				}

				await _contexto.SaveChangesAsync();
				await transaccion.CommitAsync();
			}
			catch
			{
				await transaccion.RollbackAsync();
				throw;
			}
		}

		public async Task Actualizar(
			PolizaINSAD poliza)
		{
			_contexto.Entry(poliza).State =
				EntityState.Modified;

			await _contexto.SaveChangesAsync();
		}

		public async Task Desactivar(
			int idPoliza)
		{
			PolizaINSAD poliza =
				await _contexto.PolizasINS
					.FirstOrDefaultAsync(
						p => p.IdPoliza == idPoliza)
				?? throw new InvalidOperationException(
					"No se encontró la póliza.");

			poliza.Estado = false;

			await _contexto.SaveChangesAsync();
		}

		public async Task AsignarEmpleado(
			int idPoliza,
			int idEmpleado,
			DateTime fechaAsignacion)
		{
			EmpleadoPolizaAD? asignacion =
				await _contexto.EmpleadosPolizas
					.FirstOrDefaultAsync(ep =>
						ep.IdPoliza == idPoliza &&
						ep.IdEmpleado == idEmpleado);

			if (asignacion == null)
			{
				_contexto.EmpleadosPolizas.Add(
					new EmpleadoPolizaAD
					{
						IdPoliza = idPoliza,
						IdEmpleado = idEmpleado,
						FechaAsignacion =
							fechaAsignacion.Date,
						Activa = true
					});
			}
			else
			{
				asignacion.FechaAsignacion =
					fechaAsignacion.Date;

				asignacion.FechaExclusion = null;
				asignacion.Activa = true;
			}

			await _contexto.SaveChangesAsync();
		}

		public async Task ExcluirEmpleado(
			int idPoliza,
			int idEmpleado,
			DateTime fechaExclusion)
		{
			EmpleadoPolizaAD asignacion =
				await _contexto.EmpleadosPolizas
					.FirstOrDefaultAsync(ep =>
						ep.IdPoliza == idPoliza &&
						ep.IdEmpleado == idEmpleado)
				?? throw new InvalidOperationException(
					"El empleado no está asignado a la póliza.");

			asignacion.FechaExclusion =
				fechaExclusion.Date;

			asignacion.Activa = false;

			await _contexto.SaveChangesAsync();
		}
	}
}