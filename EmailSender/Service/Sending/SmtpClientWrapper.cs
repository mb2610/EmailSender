using System.Net;
using MacroMail.Models.Configuration;
using MailKit.Net.Smtp;
using MimeKit;

namespace MacroMail.Service.Sending;

public class SmtpClientWrapper : SmtpClient, ISmtpClient
{
    public async Task SendAsync(EmailConfiguration emailConfiguration,
                                string             ipAddress,
                                MimeMessage        mimeMessage,
                                CancellationToken  token = default)
    {
        LocalEndPoint = CreateIpEndPoint(ipAddress, 0); // client.LocalDomain = "20.188.39.114";
        await ConnectAsync(emailConfiguration.Host, emailConfiguration.Port, true, token);
        await AuthenticateAsync(emailConfiguration.Email, emailConfiguration.Password, token);
        await SendAsync(mimeMessage, token);
        await DisconnectAsync(true, token);
    }

    private static IPEndPoint CreateIpEndPoint(string ipAddress, int port)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
        {
            throw new FormatException("Invalid ip address");
        }

        return new IPEndPoint(ip, port);
    }
}