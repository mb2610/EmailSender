using MacroMail.DbAccess.DataAccess;
using MacroMail.Models.Configuration;
using MacroMail.Models.Exception;
using MacroMail.Service.Builder;
using MacroMail.Service.Initialization;
using MailKit.Net.Smtp;

namespace MacroMail.Service.Sending;

public class SendingService : ISendingService
{
    private readonly IEmailConfigurationService _emailConfigurationService;
    private readonly TrackingMessageDataAccess  _trackingMessageDataAccess;
    private readonly IMimeMessageBuilder        _mimeMessageBuilder;
    private readonly RateLimiter                _rateLimiter;

    public SendingService(RateLimiter                sendingQuotaOrchestrator,
                          IEmailConfigurationService emailConfigurationService,
                          IMimeMessageBuilder        mimeMessageBuilder,
                          RateLimiter                rateLimiter,
                          TrackingMessageDataAccess  trackingMessageDataAccess)
    {
        _emailConfigurationService = emailConfigurationService;
        _mimeMessageBuilder        = mimeMessageBuilder;
        _rateLimiter               = rateLimiter;
        _trackingMessageDataAccess = trackingMessageDataAccess;
    }

    public async Task SendAsync(Guid emailMessageUid, CancellationToken token)
    {
        var emailMessage = await _trackingMessageDataAccess.GetAsync(emailMessageUid, token);
        if (emailMessage is null)
            throw new NotFoundEmailConfigurationException(emailMessageUid);

        var emailConfiguration = await _emailConfigurationService.GetConfigurationAsync(emailMessage.Sender);
        if (emailConfiguration is null)
            throw new NotFoundEmailConfigurationException(emailMessage.Sender);

        try
        {
            var ipAddress = await GetAvailableIpAddress(emailConfiguration);
        }
        catch (SendingNotAllowForEmailException e)
        {
            Console.WriteLine(e);
            return;
        }

        var message = _mimeMessageBuilder.Build(emailMessage, emailConfiguration);
        using (var emailClient = new SmtpClient())
        {
            // client.LocalDomain = "20.188.39.114";
            // emailClient.LocalEndPoint = new IPEndPoint(ipAddress, 0);
            await emailClient.ConnectAsync(emailConfiguration.Server, emailConfiguration.Port, true, token);
            await emailClient.AuthenticateAsync(emailConfiguration.Email, emailConfiguration.Password, token);
            await emailClient.SendAsync(message, token);
            await emailClient.DisconnectAsync(true, token);
        }
    }

    private async Task<string> GetAvailableIpAddress(EmailConfiguration emailConfiguration)
    {
        foreach (var ipConfig in emailConfiguration.IpConfigs)
        {
            var availableIp = await _rateLimiter.TryGetValue(ipConfig);
            if (availableIp)
                return ipConfig;
        }

        throw new SendingNotAllowForEmailException(emailConfiguration.Uid);
    }
}