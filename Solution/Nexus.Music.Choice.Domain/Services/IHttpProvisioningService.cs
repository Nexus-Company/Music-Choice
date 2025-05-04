using System.Net;

namespace Nexus.Music.Choice.Domain.Services;

public interface IHttpProvisioningService
{
    public event HttpMessageReceivedDelegate HttpMessageReceived;
    public bool IsRunning { get; }
    Task StartAsync(string uri);
    Task StopAsync();
}

public delegate Task<bool> HttpMessageReceivedDelegate(HttpProvisioningMessageEventArgs args);

public class HttpProvisioningMessageEventArgs : EventArgs
{
    public HttpListenerRequest Request { get; set; }
    public HttpProvisioningMessageEventArgs()
    {
    }
}