using MacroMail.DbAccess.DataAccess;
using MacroMail.Service.Builder;
using MacroMail.Service.Initialization;
using MacroMail.Service.Sending;
using MimeKit;
using NSubstitute;
using Xunit;

namespace MacroMail.Tests.Service.Sending;

public class SendingServiceTests
{
    [Fact]
    public async Task SendAsync__Should_send_email_When_quota_available()
    {
        // Arrange
        const string ipAddress          = "1.1.1.1";
        var          emailMessage       = ModelHelpers.EmailMessageFaker.Generate();
        var          emailConfiguration = ModelHelpers.EmailConfigurationFaker.Generate();

        var rateLimiter               = Substitute.For<IRateLimiter>();
        var pendingMessageDataAccess  = Substitute.For<IPendingMessageDataAccess>();
        var emailConfigurationService = Substitute.For<IEmailConfigurationService>();
        var trackingMessageDataAccess = Substitute.For<ITrackingMessageDataAccess>();
        var mimeMessageBuilder        = Substitute.For<IMimeMessageBuilder>();
        var sendingService = new SendingService(rateLimiter, emailConfigurationService, mimeMessageBuilder,
                                                trackingMessageDataAccess, pendingMessageDataAccess);

        rateLimiter.TryGetValue(ipAddress).Returns(true);
        pendingMessageDataAccess.GetAsync(emailMessage.Uid, CancellationToken.None).Returns(emailMessage);
        emailConfigurationService.GetConfigurationAsync(emailMessage.Sender).Returns(emailConfiguration);
        mimeMessageBuilder.Build(emailMessage, emailConfiguration).Returns(new MimeMessage());

        // Act
        await sendingService.SendAsync(emailMessage.Uid, ipAddress, CancellationToken.None);

        // Assert
        await pendingMessageDataAccess.Received(1).GetAsync(emailMessage.Uid, CancellationToken.None);
        await emailConfigurationService.Received(1).GetConfigurationAsync(emailMessage.Sender);
        mimeMessageBuilder.Received(1).Build(emailMessage, emailConfiguration);
        await trackingMessageDataAccess.Received(1)
                                       .CreateAsync(emailMessage.Uid, ipAddress, ipAddress, CancellationToken.None);
        await trackingMessageDataAccess.DidNotReceive().SetAsErrorSendingAsync(
            emailMessage.Uid, Arg.Any<string>(), CancellationToken.None);
    }
}