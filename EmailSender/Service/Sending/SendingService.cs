using System.Net;
using MacroMail.DbAccess.DataAccess;
using MacroMail.Models.Configuration;
using MacroMail.Models.Exception;
using MacroMail.Service.Builder;
using MacroMail.Service.Initialization;
using MailKit.Net.Smtp;

namespace MacroMail.Service.Sending;

public class SendingService : ISendingService
{
    private readonly IRateLimiter               _rateLimiter;
    private readonly IEmailConfigurationService _emailConfigurationService;
    private readonly IPendingMessageDataAccess  _pendingMessageDataAccess;
    private readonly TrackingMessageDataAccess  _trackingMessageDataAccess;
    private readonly IMimeMessageBuilder        _mimeMessageBuilder;

    public SendingService(IRateLimiter               rateLimiter,
                          IEmailConfigurationService emailConfigurationService,
                          IMimeMessageBuilder        mimeMessageBuilder,
                          TrackingMessageDataAccess  trackingMessageDataAccess,
                          IPendingMessageDataAccess  pendingMessageDataAccess)
    {
        _emailConfigurationService = emailConfigurationService;
        _mimeMessageBuilder        = mimeMessageBuilder;
        _rateLimiter               = rateLimiter;
        _trackingMessageDataAccess = trackingMessageDataAccess;
        _pendingMessageDataAccess  = pendingMessageDataAccess;
    }

    public async Task SendAsync(Guid messageUid, CancellationToken token)
    {
        var emailMessage = await _pendingMessageDataAccess.GetAsync(messageUid, token);
        if (emailMessage is null)
            throw new NotFoundEmailConfigurationException(messageUid);

        var emailConfiguration = await _emailConfigurationService.GetConfigurationAsync(emailMessage.Sender);
        if (emailConfiguration is null)
            throw new NotFoundEmailConfigurationException(emailMessage.Sender);

        try
        {
            var ipAddress = await GetAvailableIpAddress(emailConfiguration);
            var message   = _mimeMessageBuilder.Build(emailMessage, emailConfiguration);

            using var emailClient = new SmtpClient();
            // client.LocalDomain = "20.188.39.114";
            emailClient.LocalEndPoint = CreateIpEndPoint(ipAddress, 0);
            await emailClient.ConnectAsync(emailConfiguration.Host, emailConfiguration.Port, true, token);
            await emailClient.AuthenticateAsync(emailConfiguration.Email, emailConfiguration.Password, token);
            await emailClient.SendAsync(message, token);
            await emailClient.DisconnectAsync(true, token);

            await _trackingMessageDataAccess.CreateAsync(messageUid, ipAddress, ipAddress, token);
        }
        catch (SendingNotAllowForEmailException e)
        {
            await _trackingMessageDataAccess.SetAsErrorSendingAsync(messageUid, e.Message + e.StackTrace, token);
        }
    }

    private static IPEndPoint CreateIpEndPoint(string ipAddress, int port)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
        {
            throw new FormatException("Invalid ip-adress");
        }

        return new IPEndPoint(ip, port);
    }

    private async Task<string> GetAvailableIpAddress(EmailConfiguration emailConfiguration)
    {
        foreach (var ipConfig in emailConfiguration.AllowedHostSender)
        {
            var availableIp = await _rateLimiter.TryGetValue(ipConfig);
            if (availableIp)
                return ipConfig;
        }

        throw new SendingNotAllowForEmailException(emailConfiguration.Uid);
    }
}