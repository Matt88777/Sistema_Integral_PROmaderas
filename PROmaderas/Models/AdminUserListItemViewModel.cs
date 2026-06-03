namespace PROmaderas.UI.Models;

public class AdminUserListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public bool TieneContrasena { get; set; }
    public DateTime? UltimaInvitacionUtc { get; set; }
}
