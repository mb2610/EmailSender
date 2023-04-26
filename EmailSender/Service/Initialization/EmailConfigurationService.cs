using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using MacroMail.DbAccess.DataAccess;
using MacroMail.Models.Configuration;
using MacroMail.Models.Exception;

namespace MacroMail.Service.Initialization;

public class EmailConfigurationService : IEmailConfigurationService
{
    public static readonly ConcurrentDictionary<Guid, EmailConfiguration> EmailConfigurations = new();
    private readonly       IEmailSenderDataAccess                         _emailSenderDataAccess;

    public EmailConfigurationService(IEmailSenderDataAccess emailSenderDataAccess)
    {
        _emailSenderDataAccess = emailSenderDataAccess;
        InitAsync(CancellationToken.None).Wait();
    }

    private async Task InitAsync(CancellationToken token)
    {
        // Get available ip address
        var hostEntry = await Dns.GetHostEntryAsync(string.Empty, token);
        var ipv4Addresses = Array.FindAll(hostEntry.AddressList,
                                          address => address.AddressFamily == AddressFamily.InterNetwork)
                                 .Select(x => x.ToString())
                                 .ToList();

        // Get all senders
        var senders = (await _emailSenderDataAccess.GetSendersAsync(token)).ToList();

        // Add maching sender
        foreach (var sender in senders)
        {
            if (EmailConfigurations.ContainsKey(sender.Uid))
                continue;

            // check if exist ip config in sender
            var intersectIp = sender.AllowedHostSender.Intersect(ipv4Addresses);
            if (!intersectIp.Any())
                continue;

            EmailConfigurations.TryAdd(sender.Uid, sender);
        }
    }

    public async Task<EmailConfiguration> GetConfigurationAsync(Guid uid)
    {
        if (EmailConfigurations.TryGetValue(uid, value: out var confBeforeRetrieveAll))
            return confBeforeRetrieveAll;

        // Sender Not found 
        // Might have been added 
        await InitAsync(CancellationToken.None);

        var exist = EmailConfigurations.TryGetValue(uid, out var confAfterRetrieveAll);
        if (!exist)
            throw new NotFoundEmailConfigurationException(uid);

        return confAfterRetrieveAll;
    }

    public List<string> GetIps()
    {
        return EmailConfigurations.Values.SelectMany(x => x.AllowedHostSender).Distinct().ToList();
    }
}