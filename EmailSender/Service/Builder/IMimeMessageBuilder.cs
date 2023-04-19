using MacroMail.Models;
using MacroMail.Models.Configuration;
using MimeKit;

namespace MacroMail.Service.Builder;

public interface IMimeMessageBuilder
{
    MimeMessage Build(EmailMessage email, EmailConfiguration emailConfiguration);
}