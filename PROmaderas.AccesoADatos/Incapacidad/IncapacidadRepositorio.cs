using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Incapacidad
{
	public class IncapacidadRepositorio :
		IIncapacidadRepositorio
	{
		private readonly Contexto _contexto;

		public IncapacidadRepositorio(Contexto contexto)
		{
			_contexto = contexto;
		}

		public async Task<List<IncapacidadAD>> ObtenerTodas()
		{
			return await _contexto.Incapacidades
				.AsNoTracking()
				.Include(i => i.Empleado)
				.OrderByDescending(i => i.FechaInicio)
				.ThenByDescending(i => i.IdIncapacidad)
				.ToListAsync();
		}

		public async Task<IncapacidadAD?> ObtenerPorId(
			int idIncapacidad)
		{
			return await _contexto.Incapacidades
				.AsNoTracking()
				.Include(i => i.Empleado)
				.FirstOrDefaultAsync(
					i => i.IdIncapacidad == idIncapacidad);
		}

		public async Task<List<IncapacidadAD>> ObtenerPorEmpleado(
			int idEmpleado)
		{
			return await _contexto.Incapacidades
				.AsNoTracking()
				.Include(i => i.Empleado)
				.Where(i => i.IdEmpleado == idEmpleado)
				.OrderByDescending(i => i.FechaInicio)
				.ThenByDescending(i => i.IdIncapacidad)
				.ToListAsync();
		}

		public async Task<List<IncapacidadAD>> ObtenerPorPeriodo(
			int idEmpleado,
			DateTime fechaInicio,
			DateTime fechaFin)
		{
			DateTime inicio = fechaInicio.Date;
			DateTime fin = fechaFin.Date;

			return await _contexto.Incapacidades
				.AsNoTracking()
				.Where(i =>
					i.IdEmpleado == idEmpleado &&
					i.Activa &&
					i.FechaInicio <= fin &&
					i.FechaFin >= inicio)
				.OrderBy(i => i.FechaInicio)
				.ToListAsync();
		}

		public async Task<bool> ExisteTraslape(
			int idEmpleado,
			DateTime fechaInicio,
			DateTime fechaFin,
			int? idIncapacidadExcluir = null)
		{
			DateTime inicio = fechaInicio.Date;
			DateTime fin = fechaFin.Date;

			return await _contexto.Incapacidades
				.AsNoTracking()
				.AnyAsync(i =>
					i.IdEmpleado == idEmpleado &&
					i.Activa &&
					i.FechaInicio <= fin &&
					i.FechaFin >= inicio &&
					(!idIncapacidadExcluir.HasValue ||
					 i.IdIncapacidad !=
					 idIncapacidadExcluir.Value));
		}

		public async Task<bool> ExisteNumeroCertificado(
			string numeroCertificado,
			int? idIncapacidadExcluir = null)
		{
			string certificado =
				numeroCertificado.Trim();

			return await _contexto.Incapacidades
				.AsNoTracking()
				.AnyAsync(i =>
					i.NumeroCertificado == certificado &&
					(!idIncapacidadExcluir.HasValue ||
					 i.IdIncapacidad !=
					 idIncapacidadExcluir.Value));
		}

		public async Task Crear(
			IncapacidadAD incapacidad)
		{
			_contexto.Incapacidades.Add(incapacidad);
			await _contexto.SaveChangesAsync();
		}

		public async Task Actualizar(
			IncapacidadAD incapacidad)
		{
			_contexto.Incapacidades.Update(incapacidad);
			await _contexto.SaveChangesAsync();
		}
	}
}