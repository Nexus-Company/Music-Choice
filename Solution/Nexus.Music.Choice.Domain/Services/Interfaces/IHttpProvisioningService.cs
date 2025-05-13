using Nexus.Music.Choice.Domain.Services.EventArgs;

namespace Nexus.Music.Choice.Domain.Services.Interfaces;

public interface IHttpProvisioningServiceFactory
{
    IHttpProvisioningService GetOrCreate<T>() where T : IApiAuthenticationService;
}

public interface IHttpProvisioningService
{
    public event HttpMessageReceivedDelegate HttpMessageReceived;
    public bool IsRunning { get; }
    Task StartAsync(string uri, CancellationToken cancellationToken = default);
    Task StopAsync();
}

public delegate Task<bool> HttpMessageReceivedDelegate(HttpProvisioningMessageEventArgs args);