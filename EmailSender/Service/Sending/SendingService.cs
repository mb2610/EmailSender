using System.Net;
using MacroMail.DbAccess.DataAccess;
using MacroMail.Models.Configuration;
using MacroMail.Models.Exception;
using MacroMail.Service.Builder;
using MacroMail.Service.Initialization;
using MailKit.Net.Smtp;
using MimeKit;

namespace MacroMail.Service.Sending;

public class SendingService : ISendingService
{
    private readonly IRateLimiter               _rateLimiter;
    private readonly IEmailConfigurationService _emailConfigurationService;
    private readonly IPendingMessageDataAccess  _pendingMessageDataAccess;
    private readonly ITrackingMessageDataAccess _trackingMessageDataAccess;
    private readonly IMimeMessageBuilder        _mimeMessageBuilder;

    public SendingService(IRateLimiter               rateLimiter,
                          IEmailConfigurationService emailConfigurationService,
                          IMimeMessageBuilder        mimeMessageBuilder,
                          ITrackingMessageDataAccess trackingMessageDataAccess,
                          IPendingMessageDataAccess  pendingMessageDataAccess)
    {
        _emailConfigurationService = emailConfigurationService;
        _mimeMessageBuilder        = mimeMessageBuilder;
        _rateLimiter               = rateLimiter;
        _trackingMessageDataAccess = trackingMessageDataAccess;
        _pendingMessageDataAccess  = pendingMessageDataAccess;
    }

    public async Task SendAsync(Guid messageUid, string ipAddress, CancellationToken token)
    {
        var emailMessage = await _pendingMessageDataAccess.GetAsync(messageUid, token);
        if (emailMessage is null)
            throw new NotFoundEmailConfigurationException(messageUid);

        var emailConfiguration = await _emailConfigurationService.GetConfigurationAsync(emailMessage.Sender);
        if (emailConfiguration is null)
            throw new NotFoundEmailConfigurationException(emailMessage.Sender);

        var message      = _mimeMessageBuilder.Build(emailMessage, emailConfiguration);
        var canSendEmail = await _rateLimiter.TryGetValue(ipAddress);
        if (!canSendEmail)
            throw new NotFoundEmailConfigurationException(emailMessage.Sender);

        try
        {
            await SendEmail(ipAddress, emailConfiguration, message, token);
            await _trackingMessageDataAccess.CreateAsync(messageUid, ipAddress, ipAddress, token);
        }
        catch (Exception e)
        {
            await _trackingMessageDataAccess.SetAsErrorSendingAsync(
                messageUid, e.Message + e.StackTrace, token);
        }
    }

    private static async Task SendEmail(string             ipAddress,
                                        EmailConfiguration emailConfiguration,
                                        MimeMessage        message,
                                        CancellationToken  token = default)
    {
        var tentative = 0;
        do
        {
            try
            {
                using var emailClient = new SmtpClient();
                emailClient.LocalEndPoint = CreateIpEndPoint(ipAddress, 0); // client.LocalDomain = "20.188.39.114";
                await emailClient.ConnectAsync(emailConfiguration.Host, emailConfiguration.Port, true, token);
                await emailClient.AuthenticateAsync(emailConfiguration.Email, emailConfiguration.Password, token);
                await emailClient.SendAsync(message, token);
                await emailClient.DisconnectAsync(true, token);
                return;
            }
            catch (Exception e)
            {
                if (tentative == 3)
                    throw;
            }

            tentative++;
        } while (true);
    }

    private static IPEndPoint CreateIpEndPoint(string ipAddress, int port)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
        {
            throw new FormatException("Invalid ip-adress");
        }

        return new IPEndPoint(ip, port);
    }
}