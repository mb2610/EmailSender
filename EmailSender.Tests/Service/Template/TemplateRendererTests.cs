using FluentAssertions;
using MacroMail.Service.Template;
using Xunit;

namespace MacroMail.Tests.Service.Template;

public class TemplateRendererTests
{
    [Fact]
    public async Task ParseAsync__Should_parse_simple_template()
    {
        const string template = "sup @Model[\"Name\"] here is a list @foreach(var i in @Model[\"Numbers\"]) { @i }";
        var model = new Dictionary<string, object>
        {
            { "Name", "LUKE" },
            { "Numbers", new[] { "1", "2", "3" } }
        };
        var templateRenderer = new TemplateRenderer();
        var generated        = await templateRenderer.ParseAsync(template, model);
        generated.Should().Be("sup LUKE here is a list 123");
    }
}