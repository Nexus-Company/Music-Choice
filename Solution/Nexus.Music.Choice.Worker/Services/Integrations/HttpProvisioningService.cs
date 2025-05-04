using Nexus.Music.Choice.Domain;
using Nexus.Music.Choice.Domain.Services;
using System.Net;

namespace Nexus.Music.Choice.Worker.Services.Integrations;

internal class HttpProvisioningService : IHttpProvisioningService
{
    private readonly ILogger<HttpProvisioningService> _logger;
    private readonly IClock _clock;
    private readonly HttpListener _httpListener;

    public event HttpMessageReceivedDelegate HttpMessageReceived;
    public bool IsRunning => _httpListener.IsListening;

    Task runBackground;
    public HttpProvisioningService(IClock clock, ILogger<HttpProvisioningService> logger)
    {
        _clock = clock;
        _logger = logger;
        _httpListener = new HttpListener();
    }

    public async Task StartAsync(string url)
    {
        var baseUrl = new Uri(url).GetLeftPart(UriPartial.Authority) + '/';
        _httpListener.Prefixes.Add(baseUrl);
        _httpListener.Start();

        _logger.LogInformation("HTTP listener started at {url}", baseUrl);

        runBackground = new Task(async () =>
        {
            while (_httpListener.IsListening)
            {
                var context = await _httpListener.GetContextAsync();
                await ProcessRequestAsync(context);
            }
        });

        if (runBackground.Status == TaskStatus.Created)
            runBackground.Start();
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        _logger.LogDebug($"Received request: {context.Request.HttpMethod} {context.Request.Url}");

        bool success = await HttpMessageReceived.Invoke(new HttpProvisioningMessageEventArgs
        {
            Request = context.Request
        });

        // Send a response back to the client
    }

    public async Task StopAsync()
    {
        HttpMessageReceived = null!;
        _httpListener.Stop();
        _httpListener.Close();
    }
}