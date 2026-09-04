namespace ElaArquitetura.Domain.Common;

public abstract class Notifiable
{
    private readonly List<Notification> _notifications = new();

    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

    public bool IsValid => _notifications.Count == 0;

    protected void AddNotification(string chave, string mensagem)
        => _notifications.Add(new Notification(chave, mensagem));
}
