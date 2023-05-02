using Hangfire;
using Hangfire.States;
using MacroMail.DbAccess.DataAccess;
using MacroMail.Service.Initialization;

namespace MacroMail.Service.Sending;

public class RetrievePendingMessageService : IRetrievePendingMessageService
{
    private readonly IRateLimiter               _rateLimiter;
    private readonly IBackgroundJobClient       _backgroundJobClient;
    private readonly IPendingMessageDataAccess  _pendingMessageDataAccess;
    private readonly IEmailConfigurationService _emailConfigurationService;

    public RetrievePendingMessageService(IBackgroundJobClient       backgroundJobClient,
                                         IPendingMessageDataAccess  pendingMessageDataAccess,
                                         IEmailConfigurationService emailConfigurationService,
                                         IRateLimiter               rateLimiter)
    {
        _backgroundJobClient       = backgroundJobClient;
        _pendingMessageDataAccess  = pendingMessageDataAccess;
        _emailConfigurationService = emailConfigurationService;
        _rateLimiter               = rateLimiter;
    }

    public async Task RetrievePendingMessageJob(CancellationToken token = default)
    {
        var ipAddresses = _emailConfigurationService.GetIps();
        foreach (var ipAddress in ipAddresses)
        {
            var availableNumber = await _rateLimiter.AvailableRate(ipAddress);
            if (availableNumber <= 0)
                continue;

            var availableMessages =
                await _pendingMessageDataAccess.GetAvailableMessage(ipAddress, availableNumber, token);

            if (!availableMessages.Any())
                continue;

            var hostName = System.Net.Dns.GetHostName();
            foreach (var availableMessage in availableMessages)
                _backgroundJobClient.Create<ISendingService>(
                    job => job.SendAsync(availableMessage, ipAddress, CancellationToken.None),
                    new EnqueuedState(hostName));
        }
    }
}