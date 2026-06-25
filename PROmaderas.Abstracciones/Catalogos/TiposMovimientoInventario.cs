namespace PROmaderas.Abstracciones.Catalogos
{
    /// <summary>
    /// Literales del campo TipoMovimiento en la tabla InventarioMovimiento.
    /// Fuente única de verdad para las 3 capas (Abstracciones, AccesoADatos, LogicaDeNegocio).
    /// </summary>
    public static class TiposMovimientoInventario
    {
        // Entradas de stock
        public const string Entrada = "Entrada";       // producción registrada (INV-HU-001)
        public const string AjusteEntrada = "AjusteEntrada";

        // Salidas de stock
        public const string Salida = "Salida";         // despacho por orden de compra
        public const string AjusteSalida = "AjusteSalida";

        /// <summary>Tipos que suman al stock (se usan en la consulta de stock actual).</summary>
        public static IEnumerable<string> TiposEntrada => new[] { Entrada, AjusteEntrada };

        /// <summary>Tipos que restan al stock.</summary>
        public static IEnumerable<string> TiposSalida => new[] { Salida, AjusteSalida };
    }
}