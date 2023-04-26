using MacroMail.Models.Configuration;
using MimeKit;

namespace MacroMail.Service.Sending;

public interface ISmtpClient
{
    Task SendAsync(EmailConfiguration emailConfiguration, string ipAddress, MimeMessage mimeMessage,
                   CancellationToken  token = default);
}