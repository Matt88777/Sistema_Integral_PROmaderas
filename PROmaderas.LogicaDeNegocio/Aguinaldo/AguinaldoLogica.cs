using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Aguinaldo
{
    public class AguinaldoLogica : IAguinaldoLogica
    {
        private readonly IAguinaldoRepositorio _aguinaldoRepositorio;

        public AguinaldoLogica(IAguinaldoRepositorio aguinaldoRepositorio)
        {
            _aguinaldoRepositorio = aguinaldoRepositorio;
        }

        public async Task<List<AguinaldoResultadoAD>> CalcularPorAnio(int anio)
        {
            var desde = new DateTime(anio - 1, 12, 1);
            var hasta = new DateTime(anio, 12, 1);

            var detalles = await _aguinaldoRepositorio.ObtenerDetallesPorPeriodo(desde, hasta);
            var empleados = await _aguinaldoRepositorio.ObtenerEmpleados();

            var resultados = detalles
                .GroupBy(d => d.IdEmpleado)
                .Select(g =>
                {
                    var empleado = empleados.FirstOrDefault(e => e.IdEmpleado == g.Key);
                    var totalBruto = g.Sum(d => d.SalarioBruto);

                    return new AguinaldoResultadoAD
                    {
                        IdEmpleado = g.Key,
                        NombreEmpleado = empleado?.Nombre ?? "Desconocido",
                        Anio = anio,
                        TotalSalarioBruto = totalBruto,
                        MontoAguinaldo = Math.Round(totalBruto / 12m, 2)
                    };
                })
                .OrderBy(r => r.NombreEmpleado)
                .ToList();

            return resultados;
        }
    }
}