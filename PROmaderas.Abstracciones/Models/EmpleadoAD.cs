using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Empleado")]
public class EmpleadoAD
{
    [Key]
    public int IdEmpleado { get; set; }

    public string Nombre { get; set; }
    public string Cedula { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }

    [NotMapped]
    public string? Puesto { get; set; }

    public string Departamento { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public int IdPuesto { get; set; }
    public DateTime FechaIngreso { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}