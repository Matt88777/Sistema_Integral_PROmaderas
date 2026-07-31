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

    // PLA-HU-017: cuándo y por qué salió la persona. Las escribe la liquidación, NO el
    // formulario de Empleados: por eso EmpleadoRepositorio.Actualizar las restaura siempre
    // desde la BD (sin eso, editar el teléfono de un empleado ya liquidado le borraría la
    // fecha de salida, porque el form no manda estos campos y el Update es sobre una
    // entidad detached).
    //
    // MotivoSalida es siempre uno de MotivosSalida: la columna no tiene CHECK.
    [Column(TypeName = "date")]
    public DateTime? FechaSalida { get; set; }

    [MaxLength(50)]
    public string? MotivoSalida { get; set; }

    // Sprint 5: datos de emergencia y salud. Son INFORMATIVOS: no alimentan ningún cálculo,
    // no tienen catálogo y no afectan planilla ni liquidación.
    //
    // Todos nullable: los empleados que ya están en la base no los tienen, y son opcionales
    // por naturaleza. Los StringLength calzan EXACTO con las columnas que creó
    // scripts/PROmaderasDB_SPRINT5.sql (bloque 4): 150 / 25 / 50 / 500 / 500.
    //
    // El parentesco es texto libre a propósito, NO un combo: un <select> cuyas opciones no
    // cubran el valor que ya está en la BD cae en la opción vacía y al guardar borra el dato.
    // Es el bug que ya se comió JornadaLaboral y Departamento.
    //
    // PRIVACIDAD: alergias y medicamentos son datos de salud. EmpleadosController es
    // [Authorize(Roles = Roles.Administrador)], así que quedan restringidos al Administrador.

    [StringLength(150)]
    [Display(Name = "Nombre del contacto de emergencia")]
    public string? ContactoEmergenciaNombre { get; set; }

    [StringLength(25)]
    [Display(Name = "Teléfono del contacto")]
    public string? ContactoEmergenciaTelefono { get; set; }

    [StringLength(50)]
    [Display(Name = "Parentesco")]
    public string? ContactoEmergenciaParentesco { get; set; }

    [StringLength(500)]
    [Display(Name = "Alergias")]
    public string? Alergias { get; set; }

    [StringLength(500)]
    [Display(Name = "Medicamentos")]
    public string? Medicamentos { get; set; }
}