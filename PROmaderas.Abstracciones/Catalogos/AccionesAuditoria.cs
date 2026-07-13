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

        // PLA-HU-019: las 3 escrituras del versionado de parámetros de planilla.
        public const string CrearParametro = "Creación de parámetro de planilla";
        public const string NuevaVersionParametro = "Nueva versión de parámetro";
        public const string AnularVersionParametro = "Anulación de versión de parámetro";

        // PLA-HU-012: las 2 escrituras de vacaciones. No hay "edición": una vacación mal
        // digitada se anula y se vuelve a registrar.
        public const string RegistrarVacacion = "Registro de vacación";
        public const string AnularVacacion = "Anulación de vacación";
    }
}
