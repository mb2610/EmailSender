using FluentAssertions;
using MacroMail.Service.Builder;
using MimeKit;
using MimeKit.Text;
using Xunit;

namespace MacroMail.Tests.Service.Builder;

public class MimeMessageBuilderTests
{
    [Fact]
    public void Build__Should_build_mime_message_from_email_message()
    {
        var builder            = new MimeMessageBuilder();
        var emailMessage       = ModelHelpers.EmailMessageFaker.Generate();
        var emailConfiguration = ModelHelpers.EmailConfigurationFaker.Generate();
        var expected           = new MimeMessage();
        expected.From.Add(new MailboxAddress(string.Empty, emailConfiguration.Email));
        expected.To.Add(new MailboxAddress(string.Empty, emailMessage.To));
        expected.Subject = emailMessage.Subject;
        expected.Body = new TextPart(TextFormat.Html)
            { Text = emailMessage.Content, ContentTransferEncoding = ContentEncoding.Base64 };

        var actual = builder.Build(emailMessage, emailConfiguration);

        actual.Should()
              .BeEquivalentTo(expected, x => x.Excluding(_ => _.Headers)
                                              .Excluding(_ => _.MessageId)
                                              .Excluding(_ => _.Date));
    }
}