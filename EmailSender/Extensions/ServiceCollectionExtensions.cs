using MacroMail.Service.Builder;
using MacroMail.Service.Initialization;
using MacroMail.Service.Template;

namespace MacroMail.Extensions;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddTemplateService(this IServiceCollection services)
    {
        services.AddSingleton<ITemplateRenderer, TemplateRenderer>(_ => new TemplateRenderer());
        services.AddSingleton<InMemoryRazorLightProject>();
        return services;
    }
    
    public static IServiceCollection AddBuilderService(this IServiceCollection services)
    {
        services.AddTransient<IMimeMessageBuilder, MimeMessageBuilder>();
        return services;
    }
    
    public static IServiceCollection AddInitializationService(this IServiceCollection services)
    {
        services.AddTransient<IEmailConfigurationService, EmailConfigurationService>();
        services.AddTransient<ICreateNewSendingService, CreateNewSendingService>();
        services.AddTransient<IEmailConfigurationService, EmailConfigurationService>();
        return services;
    }
}