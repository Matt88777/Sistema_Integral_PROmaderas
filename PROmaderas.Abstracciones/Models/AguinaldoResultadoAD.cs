namespace PROmaderas.Abstracciones.Models
{
    public class AguinaldoResultadoAD
    {
        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public int Anio { get; set; }
        public decimal TotalSalarioBruto { get; set; }
        public decimal MontoAguinaldo { get; set; }
    }
}