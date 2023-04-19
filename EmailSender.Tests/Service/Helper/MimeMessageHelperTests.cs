using FluentAssertions;
using MacroMail.Service.Helper;
using MimeKit;
using MimeKit.Text;
using Xunit;

namespace MacroMail.Tests.Service.Helper;

public class MimeMessageHelperTests
{
    [Fact]
    public void SerializeAndDeserialize__Should_Serialize_And_Deserialize_And_Keep_Same_Data()
    {
        var emailMessage       = ModelHelpers.EmailMessageFaker.Generate();
        var emailConfiguration = ModelHelpers.EmailConfigurationFaker.Generate();
        var mimeMessage        = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(string.Empty, emailConfiguration.Email));
        mimeMessage.To.Add(new MailboxAddress(string.Empty, emailMessage.To));
        mimeMessage.Subject = emailMessage.Subject;
        mimeMessage.Body = new TextPart(TextFormat.Html)
            { Text = emailMessage.Content, ContentTransferEncoding = ContentEncoding.Base64 };

        var serialize   = mimeMessage.Serialize();
        var deserialize = serialize.Deserialize();

        mimeMessage.Should()
                   .BeEquivalentTo(deserialize, x =>
                                       x.Excluding(_ => _.Headers)
                                        .Excluding(_ => _.Date)
                                        .Excluding(_ => _.Body.Headers)
                                        .Excluding(_ => _.BodyParts));
    }
}