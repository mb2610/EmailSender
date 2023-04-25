using MacroMail.Models;

namespace MacroMail.DbAccess.DataAccess;

public interface ITrackingMessageDataAccess
{
    public Task CreateAsync(Guid              pendingMessageUid, string host, string ipAddress,
                            CancellationToken token);

    public Task SetAsErrorSendingAsync(Guid pendingMessageUid, string errorMessage, CancellationToken token);
    public Task SetAsReadAsync(Guid trackingUid, CancellationToken token);
    public Task<EmailMessage?> GetAsync(Guid trackingUid, CancellationToken token);
}