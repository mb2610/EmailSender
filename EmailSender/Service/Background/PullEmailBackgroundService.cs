using System.Data;
using MacroMail.Service.Sending;

namespace MacroMail.Service.Background;

public class PullEmailBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public PullEmailBackgroundService(IServiceProvider serviceProvider) { _serviceProvider = serviceProvider; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var scope   = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetService<IRetrievePendingMessageService>();

            if (service is null)
                throw new NoNullAllowedException("IRetrievePendingMessageService not implemented");

            await service.RetrievePendingMessageJob(stoppingToken);

            Thread.Sleep(TimeSpan.FromMinutes(1));
        }
    }
}