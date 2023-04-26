using FluentAssertions;
using FluentAssertions.Extensions;
using MacroMail.Service.Sending;
using Xunit;

namespace MacroMail.Tests.Service.Sending;

public class PeriodLimitTests
{
    [Theory]
    [InlineData("08:00", "18:00", "2023-04-26 8:00:00", true)]
    [InlineData("08:00", "18:00", "2023-04-26 10:00:00", true)]
    [InlineData("08:00", "18:00", "2023-04-26 18:00:00", false)]
    [InlineData("18:00", "08:00", "2023-04-26 18:00:00", true)]
    [InlineData("18:00", "08:00", "2023-04-26 20:00:00", true)]
    [InlineData("18:00", "08:00", "2023-04-27 02:00:00", true)]
    [InlineData("18:00", "08:00", "2023-04-27 08:00:00", false)]
    public void IsInInterval__Should_action_When_condition(string startUtc, string endUtc, string dateUtc,
                                                           bool   expected)
    {
        // Arrange
        var start  = TimeOnly.Parse(startUtc);
        var end    = TimeOnly.Parse(endUtc);
        var date   = DateTime.Parse(dateUtc).AsUtc();
        var period = new PeriodLimit(start, end, 250);
        // Act & Assert
        period.IsInInterval(date).Should().Be(expected);
    }
}