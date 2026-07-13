namespace PROmaderas.Abstracciones.Catalogos
{
    // Nombres EXACTOS de las filas de dbo.ParametroPlanilla (NombreParametro).
    // Única fuente de verdad: el cálculo de planilla los resuelve por acá, no con strings sueltos.
    //
    // CONVENCIÓN DE UNIDADES (equivocarse acá saca los montos 100x mal):
    //   *Porc / Porcentaje*    -> número 0..100. HAY QUE DIVIDIR ENTRE 100 al usarlo.
    //   *Factor*               -> multiplicador directo, NO se divide.
    //   *Piso / Monto*         -> colones, tal cual.
    //   *Dias / Anios / Horas* -> cantidad, tal cual.
    public static class ParametrosPlanilla
    {
        // Cantidad: divisor para sacar el valor de la hora. Se usa tal cual.
        public const string HorasMes = "HorasMes";

        // Porcentaje (10.67 = 10.67%). DIVIDIR ENTRE 100.
        public const string PorcentajeCCSS = "PorcentajeCCSS";

        // TRAMPA: la fila se llama "PorcentajeHoraExtra" pero su valor (1.5) es un FACTOR,
        // no un porcentaje: NO se divide entre 100. El nombre viene heredado del SEED y no se
        // renombra. La constante de C# sí se llama Factor* para que el código no mienta.
        public const string FactorHoraExtra = "PorcentajeHoraExtra";

        // Pisos de los tramos de renta: montos en colones, se usan tal cual.
        public const string RentaTramo1Piso = "RentaTramo1Piso";
        public const string RentaTramo2Piso = "RentaTramo2Piso";
        public const string RentaTramo3Piso = "RentaTramo3Piso";
        public const string RentaTramo4Piso = "RentaTramo4Piso";

        // Cantidad de días de vacaciones que acumula un empleado por mes trabajado. Se usa tal cual.
        public const string DiasVacacionesPorMes = "DiasVacacionesPorMes";

        // Porcentajes de los tramos de renta (10/15/20/25). DIVIDIR ENTRE 100.
        public const string RentaTramo1Porc = "RentaTramo1Porc";
        public const string RentaTramo2Porc = "RentaTramo2Porc";
        public const string RentaTramo3Porc = "RentaTramo3Porc";
        public const string RentaTramo4Porc = "RentaTramo4Porc";
    }
}
