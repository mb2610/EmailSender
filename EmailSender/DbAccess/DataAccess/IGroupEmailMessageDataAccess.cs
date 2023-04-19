namespace MacroMail.DbAccess.DataAccess;

public interface IGroupEmailMessageDataAccess
{
    Task<Guid> CreateAsync(Guid   externalUid, string subject, string            content,
                           string reply,       long   counter, CancellationToken token);

    Task SetAsStartAsync(Guid  groupUid, CancellationToken token);
    Task SetAsFinishAsync(Guid groupUid, CancellationToken token);
}