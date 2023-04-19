namespace MacroMail.Service.Sending;

public interface ISendingService
{
    Task SendAsync(Guid emailMessageUid, CancellationToken token);
}