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

        // PLA-HU-017 (liquidación).
        //
        // TRAMPA: la fila se llama "PreavisoDiasPorAnio" pero su valor (30) NO se multiplica por
        // los años trabajados. En Costa Rica el preaviso topa en 1 mes: con 2 años o con 20, son
        // 30 días igual. Multiplicarlo por la antigüedad pagaría múltiplos de más. El nombre viene
        // heredado del script y no se renombra; la constante de C# sí dice la verdad. Es el mismo
        // caso que FactorHoraExtra.
        public const string PreavisoDias = "PreavisoDiasPorAnio";

        // Cantidad: días de cesantía por año trabajado (19.5). Se usa tal cual, y este SÍ se
        // multiplica por los años (topados por CesantiaTopeAnios).
        public const string CesantiaDiasPorAnio = "CesantiaDiasPorAnio";

        // Cantidad: tope de años que se le reconocen a la cesantía (8). Se usa tal cual.
        public const string CesantiaTopeAnios = "CesantiaTopeAnios";

        // Porcentajes de los tramos de renta (10/15/20/25). DIVIDIR ENTRE 100.
        public const string RentaTramo1Porc = "RentaTramo1Porc";
        public const string RentaTramo2Porc = "RentaTramo2Porc";
        public const string RentaTramo3Porc = "RentaTramo3Porc";
        public const string RentaTramo4Porc = "RentaTramo4Porc";

		// PLA-HU-014: parámetros para calcular el pago patronal
		// durante una incapacidad.
		//
		// *DiasPatrono*: cantidad máxima de días cubiertos
		// directamente por la empresa.
		//
		// *PorcPatrono*: porcentaje del salario diario que paga
		// la empresa. Se almacena de 0 a 100 y debe dividirse entre 100.
		public const string IncapacidadCCSSDiasPatrono =
			"IncapacidadCCSSDiasPatrono";

		public const string IncapacidadCCSSPorcPatrono =
			"IncapacidadCCSSPorcPatrono";

		public const string IncapacidadINSDiasPatrono =
			"IncapacidadINSDiasPatrono";

		public const string IncapacidadINSPorcPatrono =
			"IncapacidadINSPorcPatrono";

		public const string IncapacidadMaternidadDiasPatrono =
			"IncapacidadMaternidadDiasPatrono";

		public const string IncapacidadMaternidadPorcPatrono =
			"IncapacidadMaternidadPorcPatrono";
	}
}
