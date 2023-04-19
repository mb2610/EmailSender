namespace MacroMail.Service.Template;

public interface ITemplateRenderer
{
    string       Parse(string      template, IDictionary<string, object> model);
    Task<string> ParseAsync(string template, IDictionary<string, object> model);
}