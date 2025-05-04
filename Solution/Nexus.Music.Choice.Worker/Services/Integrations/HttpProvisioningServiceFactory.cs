using Nexus.Music.Choice.Domain.Services.Interfaces;
using System.Collections.Concurrent;

namespace Nexus.Music.Choice.Worker.Services.Integrations;

public class HttpProvisioningServiceFactory : IHttpProvisioningServiceFactory, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, IHttpProvisioningService> _instances = new();

    public HttpProvisioningServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IHttpProvisioningService GetOrCreate<T>() where T : IApiAuthenticationService
    {
        return _instances.GetOrAdd(typeof(T), lType =>
        {
            using var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IHttpProvisioningService>();
        });
    }

    public void Dispose()
    {
        foreach (var instance in _instances.Values)
        {
            (instance as IDisposable)?.Dispose();
        }
    }
}
