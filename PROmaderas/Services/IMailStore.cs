namespace PROmaderas.UI.Services;

public interface IMailStore
{
    MailMessage Add(MailMessage message);
    IReadOnlyList<MailMessage> GetAll();
    MailMessage? GetById(int id);
    MailMessage? GetLatestForRecipient(string email);
}