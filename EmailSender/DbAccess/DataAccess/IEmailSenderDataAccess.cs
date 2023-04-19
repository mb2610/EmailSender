using MacroMail.Models.Configuration;

namespace MacroMail.DbAccess.DataAccess;

public interface IEmailSenderDataAccess
{
    Task<IEnumerable<EmailConfiguration>> GetSendersAsync(CancellationToken token);
}