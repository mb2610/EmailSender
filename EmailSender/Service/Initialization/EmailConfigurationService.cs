using System.Collections.Concurrent;
using MacroMail.DbAccess.DataAccess;
using MacroMail.Models.Configuration;
using MacroMail.Models.Exception;

namespace MacroMail.Service.Initialization;

public class EmailConfigurationService : IEmailConfigurationService
{
    private readonly ConcurrentDictionary<Guid, EmailConfiguration> _emailConfigurations = new();
    private readonly IEmailSenderDataAccess                         _emailSenderDataAccess;

    public EmailConfigurationService(IEmailSenderDataAccess emailSenderDataAccess)
    {
        _emailSenderDataAccess = emailSenderDataAccess;
        InitAsync(CancellationToken.None).Wait();
    }

    private async Task InitAsync(CancellationToken token)
    {
        var senders = await _emailSenderDataAccess.GetSendersAsync(token);
        if (senders.Any(sender => !_emailConfigurations.TryAdd(sender.Uid, sender)))
        {
            throw new Exception("Sender exists");
        }
    }

    public async Task<EmailConfiguration> GetConfigurationAsync(Guid uid)
    {
        if (_emailConfigurations.TryGetValue(uid, value: out var confBeforeRetrieveAll))
            return confBeforeRetrieveAll;

        await InitAsync(CancellationToken.None);
        var exist = _emailConfigurations.TryGetValue(uid, out var confAfterRetieveAll);
        if (!exist)
            throw new NotFoundEmailConfigurationException(uid);

        return confAfterRetieveAll;
    }

    public List<string> GetIps()
    {
        return _emailConfigurations.Values.SelectMany(x => x.AllowedHostSender).Distinct().ToList();
    }
}