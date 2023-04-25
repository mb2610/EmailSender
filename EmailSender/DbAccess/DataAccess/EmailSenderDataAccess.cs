using MacroMail.Models.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MacroMail.DbAccess.DataAccess;

public class EmailSenderDataAccess : IEmailSenderDataAccess
{
    private readonly IDbContextFactory<DataContext> _contextFactory;

    public EmailSenderDataAccess(IDbContextFactory<DataContext> contextFactory) { _contextFactory = contextFactory; }

    public async Task<IEnumerable<EmailConfiguration>> GetSendersAsync(CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var             senders = await context.EmailSenderDaos.ToListAsync(token);

        var results = senders.Select(sender => new EmailConfiguration
        {
            Uid               = sender.Uid,
            Port              = sender.Port,
            Host              = sender.Host,
            Email             = sender.Email,
            Password          = sender.Password,
            AllowedHostSender = sender.AllowedHostSender
        }).ToList();

        return results;
    }
}