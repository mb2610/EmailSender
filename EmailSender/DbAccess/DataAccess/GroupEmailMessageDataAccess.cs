using MacroMail.Models.Dao;
using Microsoft.EntityFrameworkCore;

namespace MacroMail.DbAccess.DataAccess;

public class GroupEmailMessageDataAccess : IGroupEmailMessageDataAccess
{
    private readonly IDbContextFactory<DataContext> _contextFactory;

    public GroupEmailMessageDataAccess(IDbContextFactory<DataContext> context) { _contextFactory = context; }

    public async Task<Guid> CreateAsync(Guid   externalUid, string subject, string            content,
                                        string reply,       long   counter, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var groupEmailMessageDao = new GroupEmailMessageDao
        {
            Uid         = Guid.NewGuid(),
            ExternalUid = externalUid,
            ReceiveDate = DateTime.UtcNow,
            Reply       = reply,
            Subject     = subject,
            Content     = content,
            Counter     = counter
        };

        await context.GroupEmails.AddAsync(groupEmailMessageDao, token);
        await context.SaveChangesAsync(token);

        return groupEmailMessageDao.Uid;
    }

    public async Task SetAsStartAsync(Guid groupUid, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var groupEmailMessage = new GroupEmailMessageDao
        {
            Uid       = groupUid,
            StartDate = DateTime.Now
        };
        context.GroupEmails.Attach(groupEmailMessage);
        context.Entry(groupEmailMessage).Property(_ => _.StartDate).IsModified = true;

        await context.SaveChangesAsync(token);
    }

    public async Task SetAsFinishAsync(Guid groupUid, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var groupEmailMessage = new GroupEmailMessageDao
        {
            Uid        = groupUid,
            FinishDate = DateTime.Now
        };
        context.GroupEmails.Attach(groupEmailMessage);
        context.Entry(groupEmailMessage).Property(_ => _.FinishDate).IsModified = true;

        await context.SaveChangesAsync(token);
    }
}