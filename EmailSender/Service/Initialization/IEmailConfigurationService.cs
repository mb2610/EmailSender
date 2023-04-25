using MacroMail.Models.Configuration;

namespace MacroMail.Service.Initialization;

public interface IEmailConfigurationService
{
    Task<EmailConfiguration> GetConfigurationAsync(Guid uid);
    List<string>                   GetIps();
}