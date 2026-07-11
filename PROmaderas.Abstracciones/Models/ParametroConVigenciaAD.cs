namespace PROmaderas.Abstracciones.Models
{
    // Fila del Index: un parámetro (por nombre) con la versión que rige a una fecha dada.
    //
    // Vigente puede ser NULL: un parámetro cuyas versiones estén todas anuladas, o cuya
    // única versión arranque a futuro, no tiene valor vigente hoy. Se lista igual —si no,
    // desaparecería de la pantalla y no habría forma de darle una versión nueva.
    public class ParametroConVigenciaAD
    {
        public string NombreParametro { get; set; } = string.Empty;
        public ParametroPlanillaAD? Vigente { get; set; }
        public int TotalVersiones { get; set; }
    }
}
