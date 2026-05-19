namespace PROmaderas.UI.Services;

public class MailMessage
{
    public int Id { get; set; }
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string ActionUrl { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}