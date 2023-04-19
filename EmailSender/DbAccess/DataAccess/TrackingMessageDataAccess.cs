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

    public async Task CreateAsync(Guid      trackingUid, Guid   groupUid,    string            externalUid,
                                  string[]? externalCcs, byte[] mimeMessage, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var externalCCsJoin = externalCcs is null || !externalCcs.Any() ? "" : string.Join('|', externalCcs);
        var trackingEmailMessage = new TrackingEmailMessageDao
        {
            Uid         = trackingUid,
            GroupUid    = groupUid,
            ExternalUid = externalUid,
            ExternalCCs = externalCCsJoin,
            MimeMessage = mimeMessage,
            IsError     = false
        };
        await context.TrackingEmails.AddAsync(trackingEmailMessage, token);
        await context.SaveChangesAsync(token);
    }

    public async Task SetAsSentAsync(Guid trackingUid, string ipAddress, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var trackingEmailMessage = new TrackingEmailMessageDao
        {
            Uid        = trackingUid,
            SentDate   = DateTime.Now,
            SentFromIp = ipAddress
        };
        context.TrackingEmails.Attach(trackingEmailMessage);
        context.Entry(trackingEmailMessage).Property(x => x.SentDate).IsModified   = true;
        context.Entry(trackingEmailMessage).Property(x => x.SentFromIp).IsModified = true;

        await context.SaveChangesAsync(token);
    }

    public async Task SetAsErrorSendingAsync(Guid trackingUid, string errorMessage, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var trackingEmailMessage = new TrackingEmailMessageDao
        {
            Uid          = trackingUid,
            ErrorMessage = errorMessage,
            IsError      = true
        };
        context.TrackingEmails.Attach(trackingEmailMessage);
        context.Entry(trackingEmailMessage).Property(_ => _.IsError).IsModified      = true;
        context.Entry(trackingEmailMessage).Property(_ => _.ErrorMessage).IsModified = true;

        await context.SaveChangesAsync(token);
    }

    public async Task SetAsReadAsync(Guid trackingUid, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var trackingEmailMessage = new TrackingEmailMessageDao
        {
            Uid            = trackingUid,
            LastOpenedDate = DateTime.Now
        };
        context.TrackingEmails.Attach(trackingEmailMessage);
        context.Entry(trackingEmailMessage).Property(_ => _.LastOpenedDate).IsModified = true;

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