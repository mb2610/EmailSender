using MacroMail.Models;

namespace MacroMail.DbAccess.DataAccess;

public interface ITrackingMessageDataAccess
{
    Task CreateAsync(Guid      trackingUid, Guid   groupUid,    string            externalUid,
                     string[]? externalCcs, byte[] mimeMessage, CancellationToken token);

    Task SetAsSentAsync(Guid         trackingUid, string ipAddress,    CancellationToken token);
    Task SetAsErrorSendingAsync(Guid trackingUid, string errorMessage, CancellationToken token);

    Task                SetAsReadAsync(Guid trackingUid, CancellationToken token);
    Task<EmailMessage?> GetAsync(Guid       trackingUid, CancellationToken token);
}