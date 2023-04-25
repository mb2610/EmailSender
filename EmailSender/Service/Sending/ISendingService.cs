namespace MacroMail.Service.Sending;

public interface ISendingService
{
    Task SendAsync(Guid messageUid, CancellationToken token);
}