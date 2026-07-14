using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.PolizasINS
{
	public class PolizaINSRepositorio : IPolizaINSRepositorio
	{
		private readonly Contexto _contexto;

		public PolizaINSRepositorio(Contexto contexto)
		{
			_contexto = contexto;
		}

		public async Task<List<PolizaINSAD>> ObtenerTodas()
		{
			return await _contexto.PolizasINS
				.AsNoTracking()
				.Include(p => p.Empleado)
				.OrderBy(p => p.FechaVencimiento)
				.ThenBy(p => p.NumeroPoliza)
				.ToListAsync();
		}

		public async Task<PolizaINSAD?> ObtenerPorId(int idPolizaINS)
		{
			return await _contexto.PolizasINS
				.AsNoTracking()
				.Include(p => p.Empleado)
				.FirstOrDefaultAsync(
					p => p.IdPolizaINS == idPolizaINS);
		}

		public async Task<List<PolizaINSAD>> ObtenerPorEmpleado(int idEmpleado)
		{
			return await _contexto.PolizasINS
				.AsNoTracking()
				.Include(p => p.Empleado)
				.Where(p => p.IdEmpleado == idEmpleado)
				.OrderByDescending(p => p.FechaInicio)
				.ThenByDescending(p => p.IdPolizaINS)
				.ToListAsync();
		}

		public async Task<PolizaINSAD?> ObtenerPolizaVigente(
			int idEmpleado,
			DateTime fechaEvaluacion)
		{
			DateTime fecha = fechaEvaluacion.Date;

			return await _contexto.PolizasINS
				.AsNoTracking()
				.Include(p => p.Empleado)
				.Where(p =>
					p.IdEmpleado == idEmpleado &&
					p.Activa &&
					p.FechaInicio <= fecha &&
					p.FechaVencimiento >= fecha)
				.OrderByDescending(p => p.FechaVencimiento)
				.FirstOrDefaultAsync();
		}

		public async Task<List<PolizaINSAD>> ObtenerProximasAVencer(
			DateTime fechaDesde,
			DateTime fechaHasta)
		{
			DateTime desde = fechaDesde.Date;
			DateTime hasta = fechaHasta.Date;

			return await _contexto.PolizasINS
				.AsNoTracking()
				.Include(p => p.Empleado)
				.Where(p =>
					p.Activa &&
					p.FechaVencimiento >= desde &&
					p.FechaVencimiento <= hasta)
				.OrderBy(p => p.FechaVencimiento)
				.ToListAsync();
		}

		public async Task<bool> ExisteNumeroPoliza(
			string numeroPoliza,
			int? idPolizaExcluir = null)
		{
			string numeroNormalizado = numeroPoliza.Trim();

			return await _contexto.PolizasINS
				.AsNoTracking()
				.AnyAsync(p =>
					p.NumeroPoliza == numeroNormalizado &&
					(!idPolizaExcluir.HasValue ||
					 p.IdPolizaINS != idPolizaExcluir.Value));
		}

		public async Task Crear(PolizaINSAD poliza)
		{
			_contexto.PolizasINS.Add(poliza);
			await _contexto.SaveChangesAsync();
		}

		public async Task Actualizar(PolizaINSAD poliza)
		{
			_contexto.PolizasINS.Update(poliza);
			await _contexto.SaveChangesAsync();
		}

		public async Task DesactivarPolizasDelEmpleado(
			int idEmpleado,
			int? idPolizaExcluir = null)
		{
			List<PolizaINSAD> polizasActivas =
				await _contexto.PolizasINS
					.Where(p =>
						p.IdEmpleado == idEmpleado &&
						p.Activa &&
						(!idPolizaExcluir.HasValue ||
						 p.IdPolizaINS != idPolizaExcluir.Value))
					.ToListAsync();

			if (polizasActivas.Count == 0)
				return;

			foreach (PolizaINSAD poliza in polizasActivas)
			{
				poliza.Activa = false;
			}

			await _contexto.SaveChangesAsync();
		}
	}
}