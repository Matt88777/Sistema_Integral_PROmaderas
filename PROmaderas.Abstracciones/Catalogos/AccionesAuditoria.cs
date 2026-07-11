namespace PROmaderas.Abstracciones.Catalogos
{
    // Literales de BitacoraAuditoria.Accion que MÁS DE UNA CAPA necesita conocer:
    // el Controller los escribe en ContextoAuditoria.Accion y la Lógica los usa
    // para releer la bitácora (ej. el motivo de anulación que muestra el Details).
    // Máx. 50 caracteres (StringLength de BitacoraAuditoriaAD.Accion).
    public static class AccionesAuditoria
    {
        public const string InactivarFactura = "Inactivación de factura";
        public const string ReactivarFactura = "Reactivación de factura";
    }
}
