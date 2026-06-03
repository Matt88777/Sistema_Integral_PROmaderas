namespace PROmaderas.Abstracciones.Models
{
    public class ContextoAuditoria
    {
        public string? UsuarioIdentityId { get; set; }
        public string? Email { get; set; }
        public string? Ip { get; set; }
        public string Accion { get; set; } = string.Empty;
    }
}
