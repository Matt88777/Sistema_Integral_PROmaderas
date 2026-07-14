namespace PROmaderas.Abstracciones.Catalogos
{
	public static class TiposIncapacidad
	{
		public const string EnfermedadComun =
			"Enfermedad común";

		public const string RiesgoTrabajo =
			"Riesgo de trabajo";

		public const string Maternidad =
			"Licencia por maternidad";

		public const string AccidenteTransito =
			"Accidente de tránsito";

		public const string Otro =
			"Otro";

		public static readonly IReadOnlyList<string> Todos =
			new List<string>
			{
				EnfermedadComun,
				RiesgoTrabajo,
				Maternidad,
				AccidenteTransito,
				Otro
			};

		public static bool EsValido(string? tipo)
		{
			if (string.IsNullOrWhiteSpace(tipo))
				return false;

			return Todos.Contains(tipo.Trim());
		}
	}
}