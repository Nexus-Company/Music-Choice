using Nexus.Music.Choice.Domain;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using System.Net;

namespace Nexus.Music.Choice.Worker.Services.Integrations;

internal class HttpProvisioningService : IHttpProvisioningService
{
    private readonly ILogger<HttpProvisioningService> _logger;
    private readonly IClock _clock;
    private readonly HttpListener _httpListener;

    public event HttpMessageReceivedDelegate HttpMessageReceived;
    public bool IsRunning => _httpListener.IsListening;

    public HttpProvisioningService(IClock clock, ILogger<HttpProvisioningService> logger)
    {
        _clock = clock;
        _logger = logger;
        _httpListener = new HttpListener();
    }

    public async Task StartAsync(string url, CancellationToken cancellationToken = default)
    {
        var baseUrl = new Uri(url).GetLeftPart(UriPartial.Authority) + '/';
        _httpListener.Prefixes.Add(baseUrl);
        _httpListener.Start();

        _logger.LogInformation("HTTP listener started at {url}", baseUrl);

        while (_httpListener.IsListening)
        {
            var context = await _httpListener.GetContextAsync();

            if (await ProcessRequestAsync(context))
                break;
        }

        await Task.CompletedTask;
    }

    private async Task<bool> ProcessRequestAsync(HttpListenerContext context)
    {
        _logger.LogDebug($"Received request: {context.Request.HttpMethod} {context.Request.Url}");

        bool success = await HttpMessageReceived.Invoke(new HttpProvisioningMessageEventArgs
        {
            Request = context.Request
        });

        // Send a response back to the client

        return success;
    }

    public async Task StopAsync()
    {
        HttpMessageReceived = null!;
        _httpListener.Stop();
        _httpListener.Close();
    }
}