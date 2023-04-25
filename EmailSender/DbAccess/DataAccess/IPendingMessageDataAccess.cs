using MacroMail.Models;

namespace MacroMail.DbAccess.DataAccess;

public interface IPendingMessageDataAccess
{
    /// <summary>
    /// Return number of pending message assign to host
    /// </summary>
    /// <param name="host">Machine host (example IP)</param>
    /// <param name="number">Number of pending message (example 10)</param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<ICollection<Guid>>
        GetAvailableMessage(string host, int number, CancellationToken token = default);

    Task<Guid> CreateAsync(Guid              groupUid,
                           string            externalUid,
                           string[]?         externalCcs,
                           byte[]            mimeMessage,
                           CancellationToken token = default);

    Task<EmailMessage> GetAsync(Guid messageUid, CancellationToken token = default);
}