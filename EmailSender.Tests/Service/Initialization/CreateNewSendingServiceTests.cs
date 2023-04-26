using FluentAssertions;
using MacroMail.DbAccess.DataAccess;
using MacroMail.Models;
using MacroMail.Service.Builder;
using MacroMail.Service.Initialization;
using MacroMail.Service.Template;
using MimeKit;
using NSubstitute;
using Xunit;

namespace MacroMail.Tests.Service.Initialization;

public class CreateNewSendingServiceTests
{
    [Fact]
    public async Task SendAsync__Should_action_When_condition()
    {
        // Arrange
        var groupEmailMessageDataAccess = Substitute.For<IGroupEmailMessageDataAccess>();
        var pendingMessageDataAccess    = Substitute.For<IPendingMessageDataAccess>();
        var emailConfigurationService   = Substitute.For<IEmailConfigurationService>();
        var mimeMessageBuilder          = Substitute.For<IMimeMessageBuilder>();
        var templateRenderer            = Substitute.For<ITemplateRenderer>();
        var createNewSendingService = new CreateNewSendingService(groupEmailMessageDataAccess, pendingMessageDataAccess,
                                                                  emailConfigurationService, mimeMessageBuilder,
                                                                  templateRenderer);
        var sendingRequest     = ModelHelpers.SendingRequestFaker.Generate();
        var emailConfiguration = ModelHelpers.EmailConfigurationFaker.Generate();
        var groupEmail         = Guid.NewGuid();

        emailConfigurationService.GetConfigurationAsync(sendingRequest.Sender).Returns(emailConfiguration);
        groupEmailMessageDataAccess.CreateAsync(sendingRequest.Sender, sendingRequest.Subject, sendingRequest.Content,
                                                sendingRequest.Reply, 1, CancellationToken.None).Returns(groupEmail);
        templateRenderer.ParseAsync(sendingRequest.Content, sendingRequest.Data).Returns(string.Empty);

        mimeMessageBuilder.Build(Arg.Any<EmailMessage>(), emailConfiguration).Returns(new MimeMessage());

        // Act
        var actual = await createNewSendingService.SendAsync(sendingRequest, CancellationToken.None);

        // Assert
        await emailConfigurationService.Received().GetConfigurationAsync(sendingRequest.Sender);
        await groupEmailMessageDataAccess.Received().CreateAsync(sendingRequest.Sender, sendingRequest.Subject,
                                                                 sendingRequest.Content,
                                                                 sendingRequest.Reply, 1, CancellationToken.None);

        await templateRenderer.Received().ParseAsync(sendingRequest.Content, sendingRequest.Data);
        mimeMessageBuilder.Received().Build(Arg.Any<EmailMessage>(), emailConfiguration);

        await pendingMessageDataAccess.Received().CreateAsync(groupEmail,
                                                              sendingRequest.To.Uid,
                                                              Array.Empty<string>(),
                                                              Arg.Any<byte[]>(),
                                                              CancellationToken.None);

        actual.Should().Be(groupEmail);
    }
}