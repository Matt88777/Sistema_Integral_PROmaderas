namespace PROmaderas.Abstracciones.Catalogos
{
    // Estados de dbo.Vacacion.Estado (PLA-HU-012).
    //
    // OJO — a diferencia de Factura.Estado, esta columna NO tiene un CHECK en la BD:
    // acepta cualquier nvarchar(50). Este catálogo es la ÚNICA fuente de verdad y la
    // única defensa contra un typo, así que ninguna capa escribe el estado con un string
    // suelto: siempre por acá.
    //
    // La tabla tiene DEFAULT ('Registrada') en Estado, pero ese default NUNCA se dispara
    // porque todos los INSERT salen con Estado explícito. 'Registrada' no es un estado del
    // dominio y no debe aparecer en ninguna fila nueva.
    public static class EstadosVacacion
    {
        public const string Disfrutada = "Disfrutada";
        public const string Anulada = "Anulada";
    }
}
