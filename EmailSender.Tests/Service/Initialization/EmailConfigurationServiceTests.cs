using System.Net;
using System.Net.Sockets;
using FluentAssertions;
using MacroMail.DbAccess.DataAccess;
using MacroMail.Service.Initialization;
using NSubstitute;
using Xunit;

namespace MacroMail.Tests.Service.Initialization;

public class EmailConfigurationServiceTests
{
    [Fact]
    public void GetAllMachineIp__Should_return_all_ip_in_machine()
    {
        // Act
        var ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList,
                                          address => address.AddressFamily == AddressFamily.InterNetwork);
        // var actual = Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.);

        // Assert
        ipv4Addresses.Should().HaveCount(2);
    }

    [Fact]
    public async Task InitAsync__Should_add_in_caching_only_matching_senders()
    {
        // Arrange
        var emailSenderDataAccess = Substitute.For<IEmailSenderDataAccess>();
        var ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList,
                                          address => address.AddressFamily == AddressFamily.InterNetwork);
        var emailConfiguration = ModelHelpers.EmailConfigurationFaker.Generate();
        emailConfiguration.AllowedHostSender = ipv4Addresses.Select(x => x.ToString()).ToArray();

        emailSenderDataAccess.GetSendersAsync(CancellationToken.None).Returns(new[] { emailConfiguration });

        // Act
        var emailConfigurationService = new EmailConfigurationService(emailSenderDataAccess);

        // Assert
        await emailSenderDataAccess.Received().GetSendersAsync(CancellationToken.None);
        EmailConfigurationService.EmailConfigurations.Should().NotBeEmpty();
        EmailConfigurationService.EmailConfigurations.Values.Should().ContainEquivalentOf(emailConfiguration);
    }

    [Fact]
    public async Task GetConfigurationAsync__Should_add_in_caching_only_matching_senders()
    {
        // Arrange
        var emailSenderDataAccess = Substitute.For<IEmailSenderDataAccess>();
        var ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList,
                                          address => address.AddressFamily == AddressFamily.InterNetwork);
        var emailConfiguration = ModelHelpers.EmailConfigurationFaker.Generate();
        emailConfiguration.AllowedHostSender = ipv4Addresses.Select(x => x.ToString()).ToArray();
        var emailConfigurationService = new EmailConfigurationService(emailSenderDataAccess);
        emailSenderDataAccess.GetSendersAsync(CancellationToken.None).Returns(new[] { emailConfiguration });

        // Act
        var actual = await emailConfigurationService.GetConfigurationAsync(emailConfiguration.Uid);

        // Assert
        await emailSenderDataAccess.Received(2).GetSendersAsync(CancellationToken.None);
        EmailConfigurationService.EmailConfigurations.Should().NotBeEmpty();
        EmailConfigurationService.EmailConfigurations.Values.Should().ContainEquivalentOf(emailConfiguration);
        actual.Should().BeEquivalentTo(emailConfiguration);
    }
    
    [Fact]
    public async Task GetIps__Should_add_in_caching_only_matching_senders()
    {
        // Arrange
        var emailSenderDataAccess = Substitute.For<IEmailSenderDataAccess>();
        var ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList,
                                          address => address.AddressFamily == AddressFamily.InterNetwork);
        var emailConfiguration = ModelHelpers.EmailConfigurationFaker.Generate();
        emailConfiguration.AllowedHostSender = ipv4Addresses.Select(x => x.ToString()).ToArray();
        emailSenderDataAccess.GetSendersAsync(CancellationToken.None).Returns(new[] { emailConfiguration });
        var emailConfigurationService = new EmailConfigurationService(emailSenderDataAccess);

        // Act
        var actual = emailConfigurationService.GetIps();

        // Assert
        await emailSenderDataAccess.Received(1).GetSendersAsync(CancellationToken.None);
        EmailConfigurationService.EmailConfigurations.Should().NotBeEmpty();
        EmailConfigurationService.EmailConfigurations.Values.Should().ContainEquivalentOf(emailConfiguration);
        actual.Should().BeEquivalentTo(ipv4Addresses.Select(x => x.ToString()).ToArray());
    }
}