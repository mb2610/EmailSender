using Hangfire;
using MacroMail.DbAccess.DataAccess;
using MacroMail.Service.Initialization;

namespace MacroMail.Service.Sending;

public interface IRetrievePendingMessageService
{
    public Task RetrievePendingMessageJob(CancellationToken token = default);
}

public class RetrievePendingMessageService : IRetrievePendingMessageService
{
    private readonly IRateLimiter               _rateLimiter;
    private readonly IBackgroundJobClient       _backgroundJobClient;
    private readonly IPendingMessageDataAccess  _pendingMessageDataAccess;
    private readonly IEmailConfigurationService _emailConfigurationService;

    public RetrievePendingMessageService(IBackgroundJobClient       backgroundJobClient,
                                         IPendingMessageDataAccess  pendingMessageDataAccess,
                                         IEmailConfigurationService emailConfigurationService, IRateLimiter rateLimiter)
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
            var availableIp = await _rateLimiter.TryGetValue(ipAddress);
            if (!availableIp)
                continue;

            var availableNumber = await _rateLimiter.AvailableRate(ipAddress);
            var availableMessages =
                await _pendingMessageDataAccess.GetAvailableMessage(ipAddress, availableNumber, token);

            foreach (var availableMessage in availableMessages)
                _backgroundJobClient.Enqueue<ISendingService>(
                    job => job.SendAsync(availableMessage.Uid, CancellationToken.None));
        }
    }
}