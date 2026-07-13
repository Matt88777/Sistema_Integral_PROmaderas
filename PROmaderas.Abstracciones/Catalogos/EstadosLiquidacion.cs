namespace PROmaderas.Abstracciones.Catalogos
{
    // Literales EXACTOS del CHECK CK_Liquidacion_Estado (PROmaderasDB_SPRINT4.sql).
    // A diferencia de Vacacion.Estado, esta columna SÍ tiene CHECK en la BD: el caso es
    // idéntico al de EstadosFactura, y este catálogo es su espejo en C#. Un literal que no
    // esté acá lo rechaza SQL Server con un error de constraint.
    public static class EstadosLiquidacion
    {
        // PLA-HU-017 solo usa estos dos: se calcula y se guarda, o se anula.
        public const string Calculada = "Calculada";
        public const string Anulada = "Anulada";

        // El CHECK también los permite, pero la HU NO pide flujo de aprobación ni de pago.
        // Quedan declarados para las HUs que lo agreguen; hoy no los escribe nadie.
        public const string Aprobada = "Aprobada";
        public const string Pagada = "Pagada";
    }
}
