namespace PROmaderas.Abstracciones.Catalogos
{
    // Literales EXACTOS de dbo.Empleado.MotivoSalida y dbo.Liquidacion.MotivoSalida (PLA-HU-017).
    //
    // La columna NO tiene CHECK en la BD (decisión del script: las tablas de RRHH no usan CHECK).
    // Este catálogo es la ÚNICA defensa contra un typo, y acá un typo no rompe una pantalla:
    // cambia si una persona cobra o no cobra la cesantía. Nunca se escriben strings sueltos.
    //
    // El derecho a preaviso y cesantía sale del Código de Trabajo de Costa Rica y lo documenta
    // el propio script (sección 3):
    //
    //   Despido con responsabilidad  -> preaviso SI, cesantía SI
    //   Despido sin responsabilidad  -> preaviso NO, cesantía NO
    //   Renuncia                     -> preaviso NO, cesantía NO
    //   Mutuo acuerdo                -> preaviso NO, cesantía SI
    //   Pension                      -> preaviso NO, cesantía SI
    //
    // Las VACACIONES pendientes y el AGUINALDO proporcional se pagan SIEMPRE, con cualquier
    // motivo: son salario ya devengado, no una indemnización.
    public static class MotivosSalida
    {
        public const string DespidoConResponsabilidad = "Despido con responsabilidad";
        public const string DespidoSinResponsabilidad = "Despido sin responsabilidad";
        public const string Renuncia = "Renuncia";
        public const string MutuoAcuerdo = "Mutuo acuerdo";
        public const string Pension = "Pension";   // sin tilde: así está sembrado en el script

        // Para poblar el combo de la pantalla sin repetir los literales en la vista.
        public static readonly string[] Todos =
        {
            DespidoConResponsabilidad,
            DespidoSinResponsabilidad,
            Renuncia,
            MutuoAcuerdo,
            Pension
        };

        public static bool EsValido(string? motivo)
            => motivo != null && Todos.Contains(motivo);

        // Solo el despido CON responsabilidad patronal genera preaviso.
        public static bool GeneraPreaviso(string motivo)
        {
            Validar(motivo);
            return motivo == DespidoConResponsabilidad;
        }

        public static bool GeneraCesantia(string motivo)
        {
            Validar(motivo);
            return motivo is DespidoConResponsabilidad or MutuoAcuerdo or Pension;
        }

        // Un motivo desconocido es un BUG, no un "no tiene derecho". Devolver false en silencio
        // haría que un typo se pague como una renuncia: la persona se quedaría sin su cesantía
        // y nadie se enteraría. Que reviente.
        private static void Validar(string motivo)
        {
            if (!EsValido(motivo))
                throw new ArgumentException(
                    $"El motivo de salida '{motivo}' no es válido. " +
                    $"Los motivos permitidos son: {string.Join(", ", Todos)}.");
        }
    }
}
