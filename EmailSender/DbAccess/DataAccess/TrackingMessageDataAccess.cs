using MacroMail.Models;
using MacroMail.Models.Dao;
using Microsoft.EntityFrameworkCore;

namespace MacroMail.DbAccess.DataAccess;

public class TrackingMessageDataAccess : ITrackingMessageDataAccess
{
    private readonly IDbContextFactory<DataContext> _contextFactory;

    public TrackingMessageDataAccess(IDbContextFactory<DataContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task CreateAsync(Guid              pendingMessageUid, string host, string ipAddress,
                                  CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var pendingMessage = await context.PendingEmails.SingleAsync(x => x.Uid == pendingMessageUid, token);

        var trackingEmailMessage = new TrackingMessageDao
        {
            Uid         = pendingMessage.Uid,
            GroupUid    = pendingMessage.GroupUid,
            ExternalUid = pendingMessage.ExternalUid,
            ExternalCCs = pendingMessage.ExternalCCs,
            MimeMessage = pendingMessage.MimeMessage,
            SentDate    = DateTime.Now,
            SentHost    = host
        };
        await context.TrackingEmails.AddAsync(trackingEmailMessage, token);
        context.PendingEmails.Remove(pendingMessage);
        await context.SaveChangesAsync(token);
    }

    public async Task SetAsErrorSendingAsync(Guid pendingMessageUid, string errorMessage, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var pendingMessage = await context.PendingEmails.SingleAsync(x => x.Uid == pendingMessageUid, token);

        var trackingEmailMessage = new TrackingMessageDao
        {
            Uid            = pendingMessage.Uid,
            GroupUid       = pendingMessage.GroupUid,
            ExternalUid    = pendingMessage.ExternalUid,
            ExternalCCs    = pendingMessage.ExternalCCs,
            MimeMessage    = pendingMessage.MimeMessage,
            IsErrorSending = true,
            ErrorMessage   = errorMessage
        };
        await context.TrackingEmails.AddAsync(trackingEmailMessage, token);
        context.PendingEmails.Remove(pendingMessage);
        await context.SaveChangesAsync(token);
    }

    public async Task SetAsReadAsync(Guid trackingUid, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var trackingEmailMessage = await context.TrackingEmails.SingleAsync(x => x.Uid == trackingUid, token);

        trackingEmailMessage.LastOpenedDate = DateTime.Now;
        trackingEmailMessage.NumberOpening++;

        context.TrackingEmails.Update(trackingEmailMessage);
        await context.SaveChangesAsync(token);
    }

    public async Task<EmailMessage?> GetAsync(Guid trackingUid, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var results = await context.TrackingEmails
                                   .Include(x => x.Group)
                                   .Where(x => !x.SentDate.HasValue && x.Uid == trackingUid)
                                   .Select(x => new EmailMessage
                                    {
                                        Uid     = x.Uid,
                                        Sender  = x.Group.ExternalUid,
                                        Subject = x.Group.Subject,
                                        Content = x.Group.Content,
                                        To      = x.ExternalUid
                                    })
                                   .SingleOrDefaultAsync(token);

        return results;
    }
}