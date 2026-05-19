using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROmaderas.Abstracciones.Models
{
	[Table("Cliente")]
	public class ClienteAD
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "El nombre es requerido")]
    	[StringLength(150)]
    	[MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
		public string Nombre { get; set; } = string.Empty;

		[Required(ErrorMessage = "La cédula es requerida")]
    	[StringLength(20)]
    	[RegularExpression(@"^\d{6,20}$", ErrorMessage = "La cédula debe contener sólo dígitos (6-20 caracteres)")]
		public string Cedula { get; set; } = string.Empty;

		[Required(ErrorMessage = "El correo es requerido")]
		[StringLength(150)]
		[EmailAddress(ErrorMessage = "Correo inválido")]
		public string Correo { get; set; } = string.Empty;

    	[StringLength(20)]
    	[RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Teléfono inválido. Use sólo dígitos, opcionalmente con + y entre 7 y 15 caracteres")]
    	public string? Telefono { get; set; }

		[StringLength(300)]
		public string? Direccion { get; set; }

		[StringLength(450)]
		public string? UsuarioIdentityId { get; set; }
		
		public virtual ICollection<PedidoAD>? Pedidos { get; set; }
	}
}