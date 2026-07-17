namespace PROmaderas.Abstracciones.Catalogos
{
    public static class PeriodosReporteVentas
    {
        public const string Diario = "Diario";
        public const string Semanal = "Semanal";
        public const string Mensual = "Mensual";

        public static IEnumerable<string> Todos => new[] { Diario, Semanal, Mensual };

        public static bool EsValido(string? tipoPeriodo) =>
            !string.IsNullOrWhiteSpace(tipoPeriodo) && Todos.Contains(tipoPeriodo);
    }
}