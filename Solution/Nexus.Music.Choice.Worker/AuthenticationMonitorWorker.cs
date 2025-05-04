using Nexus.Music.Choice.Domain.Services;

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
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var integrationServices = scope.ServiceProvider.GetServices<IApiAuthenticationService>();
            foreach (var service in integrationServices)
            {
                try
                {
                    bool authenticated = await service.CheckAuthenticationAsync();

                    if (authenticated)
                        continue;

                    _logger.LogWarning("The service '{service}' require authentication.", service.Name);

                    await service.StartAuthenticationAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error while checking auth for {service.Name}");
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