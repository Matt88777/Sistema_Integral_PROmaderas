using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Empleado")]
public class EmpleadoAD
{
    [Key]
    public int? IdEmpleado { get; set; }

    public string? Nombre { get; set; }
    public string? Cedula { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }

    [NotMapped]
    public string? Puesto { get; set; }

    public string? Departamento { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public int IdPuesto { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public bool? Estado { get; set; }
    public DateTime? FechaCreacion { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SalarioBase { get; set; }

    [MaxLength(50)]
    public string? TipoPago { get; set; }

    [MaxLength(50)]
    public string? JornadaLaboral { get; set; }

    // PLA-HU-012: días de vacaciones que el empleado ya traía al migrarse desde Excel.
    // Es el punto de partida del acumulado; lo trabajado en el sistema se le suma encima.
    // El tope es la capacidad real de la columna, decimal(10,2). Negativo no tiene sentido:
    // un saldo inicial en contra no es algo que el negocio contemple.
    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 99999999.99, ErrorMessage = "El saldo de vacaciones inicial no puede ser negativo.")]
    [Display(Name = "Saldo de vacaciones inicial (días)")]
    public decimal SaldoVacacionesInicial { get; set; }
}