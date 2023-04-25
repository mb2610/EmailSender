namespace MacroMail.Service.Sending;

public interface ISendingService
{
    Task SendAsync(Guid messageUid, string ipAddress, CancellationToken token = default);
}