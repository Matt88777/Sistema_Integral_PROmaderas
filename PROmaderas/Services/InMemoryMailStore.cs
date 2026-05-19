namespace PROmaderas.UI.Services;

public class InMemoryMailStore : IMailStore
{
    private readonly List<MailMessage> _messages = [];
    private readonly object _sync = new();
    private int _nextId = 1;

    public MailMessage Add(MailMessage message)
    {
        lock (_sync)
        {
            message.Id = _nextId++;
            message.CreatedAtUtc = DateTime.UtcNow;
            _messages.Insert(0, message);
            return message;
        }
    }

    public IReadOnlyList<MailMessage> GetAll()
    {
        lock (_sync)
        {
            return _messages.ToList();
        }
    }

    public MailMessage? GetById(int id)
    {
        lock (_sync)
        {
            return _messages.FirstOrDefault(x => x.Id == id);
        }
    }

    public MailMessage? GetLatestForRecipient(string email)
    {
        lock (_sync)
        {
            return _messages.FirstOrDefault(x => x.To.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}