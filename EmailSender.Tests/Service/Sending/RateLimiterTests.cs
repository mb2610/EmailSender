using FluentAssertions;
using MacroMail.Service.Sending;
using Xunit;

namespace MacroMail.Tests.Service.Sending;

public class RateLimiterTests
{
    [Fact]
    public async Task TryGetValue__Should_return_true_when_first_sent_email_for_ip()
    {
        // Arrange
        var rateLimiter = new RateLimiter(TimeSpan.FromMinutes(1));

        // Act
        var actual = await rateLimiter.TryGetValue("1.1.1.1");

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public async Task TryGetValue__Should_return_true_when_secondly_sent_email()
    {
        // Arrange
        var rateLimiter = new RateLimiter(TimeSpan.FromMinutes(1));
        await rateLimiter.TryGetValue("1.1.1.1");

        // Act
        var actual = await rateLimiter.TryGetValue("1.1.1.1");

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public async Task TryGetValue__Should_return_false_when_quota_reached()
    {
        // Arrange
        var rateLimiter = new RateLimiter(TimeSpan.FromHours(1));

        var tasks = Enumerable.Range(0, 250).Select(_ => rateLimiter.TryGetValue("1.1.1.1")).ToArray();
        await Task.WhenAll(tasks);
        
        // Act
        var actual = await rateLimiter.TryGetValue("1.1.1.1");

        // Assert
        actual.Should().BeFalse();
    }
}