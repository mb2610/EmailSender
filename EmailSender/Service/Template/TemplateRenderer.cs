using System.Security.Cryptography;
using System.Text;
using RazorLight;

namespace MacroMail.Service.Template;

public class TemplateRenderer : ITemplateRenderer
{
    private readonly RazorLightEngine _engine;

    public TemplateRenderer(string? root = null)
    {
        _engine = new RazorLightEngineBuilder()
                 .UseFileSystemProject(root ?? Directory.GetCurrentDirectory())
                 .UseMemoryCachingProvider()
                 .Build();
    }

    public string Parse(string template, IDictionary<string, object> model)
    {
        return ParseAsync(template, model).GetAwaiter().GetResult();
    }

    public async Task<string> ParseAsync(string template, IDictionary<string, object> model)
    {
        return await _engine.CompileRenderStringAsync(GetHashString(template), template, model);
    }

    private static string GetHashString(string inputString)
    {
        var sb        = new StringBuilder();
        var hashbytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(inputString));
        foreach (var b in hashbytes) sb.Append(b.ToString("X2"));

        return sb.ToString();
    }
}