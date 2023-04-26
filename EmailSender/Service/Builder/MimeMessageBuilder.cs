using MacroMail.Models;
using MacroMail.Models.Configuration;
using MimeKit;
using MimeKit.Text;

namespace MacroMail.Service.Builder;

public class MimeMessageBuilder : IMimeMessageBuilder
{
    public MimeMessage Build(EmailMessage email, EmailConfiguration emailConfiguration)
    {
        var message = new MimeMessage();
        message.MessageId = email.Uid.ToString();
        message.From.Add(new MailboxAddress("", emailConfiguration.Email));
        message.To.Add(new MailboxAddress("", email.To));
        if (!string.IsNullOrEmpty(email.Reply))
            message.ReplyTo.Add(new MailboxAddress("", email.Reply));
        if (email.Ccs != null && email.Ccs.Any())
            foreach (var cc in email.Ccs)
                message.Cc.Add(new MailboxAddress("", cc));
        message.Subject = email.Subject;
        message.Body = new TextPart(TextFormat.Html)
        {
            ContentTransferEncoding = ContentEncoding.Base64,
            Text                    = email.Content
        };
        return message;
    }
}