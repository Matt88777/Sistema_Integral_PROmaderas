namespace PROmaderas.Abstracciones.Catalogos
{
    // Catálogo de formas de pago de FAC-HU-004.
    // El esquema (PagoFactura.FormaPago NVARCHAR(50)) NO tiene CHECK, así que estos
    // valores no provienen de la BD: son decisión de negocio y la única fuente de verdad.
    // Vive en Abstracciones para ser alcanzable por las 3 capas (igual que EstadosFactura).
    public static class FormasPago
    {
        public const string Efectivo = "Efectivo";
        public const string Transferencia = "Transferencia";
        public const string Cheque = "Cheque";
        public const string SinpeMovil = "SINPE Móvil";
        public const string Tarjeta = "Tarjeta";

        // Lista completa para poblar el dropdown y validar pertenencia.
        public static IEnumerable<string> Todas => new[]
        {
            Efectivo, Transferencia, Cheque, SinpeMovil, Tarjeta
        };
    }
}
