using MacroMail.Models;
using MacroMail.Models.Dao;
using Microsoft.EntityFrameworkCore;

namespace MacroMail.DbAccess.DataAccess;

public class PendingMessageDataAccess : IPendingMessageDataAccess
{
    private readonly IDbContextFactory<DataContext> _contextFactory;

    public PendingMessageDataAccess(IDbContextFactory<DataContext> contextFactory) { _contextFactory = contextFactory; }

    public async Task<ICollection<Guid>> GetAvailableMessage(
        string host, int number, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);

        var pendingList = await context.PendingEmails
                                       .Include(x => x.Group.Sender)
                                       .AsNoTracking()
                                       .Where(x => x.Group.Sender.AllowedHostSender.Contains(host))
                                       .OrderByDescending(x => x.Group.Priority)
                                       .ThenBy(x => x.Group.ReceiveDate)
                                       .Take(number)
                                       .ToListAsync(token);

        // TODO ASSIGNE
        // TO NOT TAKE IT AGAIN
        
        return pendingList.Select(x => x.Uid).ToList();
    }

    public async Task<Guid> CreateAsync(Guid              groupUid,
                                        string            externalUid,
                                        string[]?         externalCcs,
                                        byte[]            mimeMessage,
                                        CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var pendingEmail = new PendingMessageDao
        {
            Uid         = Guid.NewGuid(),
            GroupUid    = groupUid,
            ExternalUid = externalUid,
            ExternalCCs = externalCcs,
            MimeMessage = mimeMessage
        };
        await context.PendingEmails.AddAsync(pendingEmail, token);
        await context.SaveChangesAsync(token);

        return pendingEmail.Uid;
    }

    public async Task<EmailMessage> GetAsync(Guid messageUid, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        return await context.PendingEmails
                            .Include(x => x.Group)
                            .Select(x => new EmailMessage
                             {
                                 Uid     = x.Uid,
                                 Sender  = x.Group.ExternalUid,
                                 Subject = x.Group.Subject,
                                 Content = x.Group.Content,
                                 To      = x.ExternalUid
                             })
                            .SingleAsync(x => x.Uid == messageUid, cancellationToken: token);
    }
}