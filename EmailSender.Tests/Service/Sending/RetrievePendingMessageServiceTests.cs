using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using MacroMail.DbAccess.DataAccess;
using MacroMail.Service.Initialization;
using MacroMail.Service.Sending;
using NSubstitute;
using Xunit;

namespace MacroMail.Tests.Service.Sending;

public class RetrievePendingMessageServiceTests
{
    [Fact]
    public async Task RetrievePendingMessageJob__Should_do_nothing_if_not_available_quota()
    {
        // Arrange
        const string ipAddress = "1.1.1.1";

        var rateLimiter               = Substitute.For<IRateLimiter>();
        var pendingMessageDataAccess  = Substitute.For<IPendingMessageDataAccess>();
        var emailConfigurationService = Substitute.For<IEmailConfigurationService>();
        var backgroundJobClient       = Substitute.For<IBackgroundJobClient>();

        var retrievePendingMessageService = new RetrievePendingMessageService(
            backgroundJobClient, pendingMessageDataAccess,
            emailConfigurationService, rateLimiter);
        emailConfigurationService.GetIps().Returns(new List<string> { ipAddress });
        rateLimiter.AvailableRate(ipAddress).Returns(0);

        // Act
        await retrievePendingMessageService.RetrievePendingMessageJob(CancellationToken.None);

        // Assert
        emailConfigurationService.GetIps();
        await rateLimiter.Received(1).AvailableRate(ipAddress);
        await pendingMessageDataAccess.DidNotReceive()
                                      .GetAvailableMessage(ipAddress, Arg.Any<int>(), CancellationToken.None);
        backgroundJobClient.DidNotReceiveWithAnyArgs();
    }

    [Fact]
    public async Task RetrievePendingMessageJob__Should_do_nothing_if_no_pending_email()
    {
        // Arrange
        const string ipAddress = "1.1.1.1";

        var rateLimiter               = Substitute.For<IRateLimiter>();
        var pendingMessageDataAccess  = Substitute.For<IPendingMessageDataAccess>();
        var emailConfigurationService = Substitute.For<IEmailConfigurationService>();
        var backgroundJobClient       = Substitute.For<IBackgroundJobClient>();

        var retrievePendingMessageService = new RetrievePendingMessageService(
            backgroundJobClient, pendingMessageDataAccess,
            emailConfigurationService, rateLimiter);

        emailConfigurationService.GetIps().Returns(new List<string> { ipAddress });
        rateLimiter.AvailableRate(ipAddress).Returns(1);
        pendingMessageDataAccess.GetAvailableMessage(ipAddress, 1, CancellationToken.None)
                                .Returns(Array.Empty<Guid>());

        // Act
        await retrievePendingMessageService.RetrievePendingMessageJob(CancellationToken.None);

        // Assert
        emailConfigurationService.GetIps();
        await rateLimiter.Received(1).AvailableRate(ipAddress);
        await pendingMessageDataAccess.Received(1).GetAvailableMessage(ipAddress, 1, CancellationToken.None);
        backgroundJobClient.DidNotReceiveWithAnyArgs();
    }

    [Fact]
    public async Task RetrievePendingMessageJob__Should_create_job_if_exist_quota_and_available_pending_email()
    {
        // Arrange
        const string ipAddress    = "1.1.1.1";
        var          pendingEmail = Guid.NewGuid();

        var rateLimiter               = Substitute.For<IRateLimiter>();
        var pendingMessageDataAccess  = Substitute.For<IPendingMessageDataAccess>();
        var emailConfigurationService = Substitute.For<IEmailConfigurationService>();
        var backgroundJobClient       = Substitute.For<IBackgroundJobClient>();

        var retrievePendingMessageService = new RetrievePendingMessageService(
            backgroundJobClient, pendingMessageDataAccess,
            emailConfigurationService, rateLimiter);

        emailConfigurationService.GetIps().Returns(new List<string> { ipAddress });
        rateLimiter.AvailableRate(ipAddress).Returns(1);
        pendingMessageDataAccess.GetAvailableMessage(ipAddress, 1, CancellationToken.None)
                                .Returns(new[] { pendingEmail });

        // Act
        await retrievePendingMessageService.RetrievePendingMessageJob(CancellationToken.None);

        // Assert
        emailConfigurationService.GetIps();
        await rateLimiter.Received(1).AvailableRate(ipAddress);
        await pendingMessageDataAccess.Received(1).GetAvailableMessage(ipAddress, 1, CancellationToken.None);
        backgroundJobClient.Received(1).Create(Arg.Is<Job>(job =>
                                                               job.Type           == typeof(ISendingService)
                                                               && job.Method.Name == "SendAsync"
                                                               && job.Args.Count  == 3
                                                               && job.Args[0].Equals(pendingEmail)
                                                               && job.Args[1].Equals(ipAddress)
                                                               && job.Args[2].Equals(CancellationToken.None)),
                                               Arg.Any<EnqueuedState>());
    }

    [Fact]
    public async Task
        RetrievePendingMessageJob__Should_create_250_job_if_exist_quota_is_max_and_available_pending_email_is_250()
    {
        // Arrange
        const string ipAddress                 = "1.1.1.1";
        var          rateLimiter               = Substitute.For<IRateLimiter>();
        var          pendingMessageDataAccess  = Substitute.For<IPendingMessageDataAccess>();
        var          emailConfigurationService = Substitute.For<IEmailConfigurationService>();
        var          backgroundJobClient       = Substitute.For<IBackgroundJobClient>();

        var retrievePendingMessageService = new RetrievePendingMessageService(
            backgroundJobClient, pendingMessageDataAccess,
            emailConfigurationService, rateLimiter);

        emailConfigurationService.GetIps().Returns(new List<string> { ipAddress });
        rateLimiter.AvailableRate(ipAddress).Returns(250);
        pendingMessageDataAccess.GetAvailableMessage(ipAddress, 250, CancellationToken.None)
                                .Returns(Enumerable.Range(0, 250).Select(_ => Guid.NewGuid()).ToList());

        // Act
        await retrievePendingMessageService.RetrievePendingMessageJob(CancellationToken.None);

        // Assert
        emailConfigurationService.GetIps();
        await rateLimiter.Received(1).AvailableRate(ipAddress);
        await pendingMessageDataAccess.Received(1).GetAvailableMessage(ipAddress, 250, CancellationToken.None);
        backgroundJobClient.Received(250).Create(Arg.Any<Job>(), Arg.Any<EnqueuedState>());
    }
}