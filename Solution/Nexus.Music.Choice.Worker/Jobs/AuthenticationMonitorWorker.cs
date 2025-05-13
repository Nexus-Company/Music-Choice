using Nexus.Music.Choice.Domain.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Jobs;

public class AuthenticationMonitorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuthenticationMonitorWorker> _logger;

    public AuthenticationMonitorWorker(IServiceProvider serviceProvider, ILogger<AuthenticationMonitorWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var integrationServices = scope.ServiceProvider.GetServices<IApiAuthenticationService>();

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var service in integrationServices)
            {
                try
                {
                    bool authenticated = await service.CheckAuthenticationAsync();

                    if (authenticated)
                        continue;

                    _logger.LogWarning("The service '{ServiceName}' requires authentication.", service.Name);

                    await service.StartAuthenticationAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking auth for {ServiceName}", service.Name);
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}