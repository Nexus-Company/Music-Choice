using System.Net;

namespace Nexus.Music.Choice.Domain.Services.EventArgs;

public class HttpProvisioningMessageEventArgs : System.EventArgs
{
    public HttpListenerRequest Request { get; set; }
    public HttpProvisioningMessageEventArgs()
    {
    }
}