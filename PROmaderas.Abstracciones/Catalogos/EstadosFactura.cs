namespace PROmaderas.Abstracciones.Catalogos
{
    // Literales EXACTOS del CHECK CK_Factura_Estado (PROmaderasDB_NEW.sql).
    // Única fuente de verdad de los estados de factura para las 3 capas.
    // Vive en Abstracciones (no junto a Roles, que es de la UI) porque la
    // Lógica y el Acceso a Datos también validan/usan estos valores y no
    // referencian el proyecto de UI.
    public static class EstadosFactura
    {
        public const string Emitida = "Emitida";
        public const string PendienteDePago = "Pendiente de Pago"; // P mayúscula, exacto al CHECK
        public const string Pagada = "Pagada";
        public const string Anulada = "Anulada";
    }
}
