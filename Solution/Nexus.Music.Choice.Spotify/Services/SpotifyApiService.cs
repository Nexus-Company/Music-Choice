using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexus.Music.Choice.Domain;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Models;
using Nexus.Music.Choice.Spotify.Services.Interfaces;
using System.Diagnostics;

namespace Nexus.Music.Choice.Spotify.Services;

internal class SpotifyApiService : ISpotifyApiService, IDisposable
{
    private const string SpotifyEndPoint = "https://api.spotify.com/v1";

    private readonly ISpotifyTokenStoreService _spotifyTokenStoreService;
    private readonly ILogger<SpotifyApiService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IClock _clock;
    public SpotifyApiService(
        ISpotifyTokenStoreService authenticationService,
        HttpClient httpClient,
        IClock clock,
        ILogger<SpotifyApiService> logger)
    {
        _spotifyTokenStoreService = authenticationService;
        _httpClient = httpClient;
        _logger = logger;
        _clock = clock;

        _spotifyTokenStoreService.AccessTokenChanged += AccessTokenChanged;

        GetAuthorizationToken();
    }

    public async Task<SpotifyPlayerState?> GetPlayerStateAsync(CancellationToken stoppingToken = default)
    {
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri($"{SpotifyEndPoint}/me/player")
        };

        var stopwatch = Stopwatch.StartNew();

        var response = await _httpClient.SendAsync(request, stoppingToken);
        string contentStr = await response.Content.ReadAsStringAsync(stoppingToken);
        var playerState = JsonConvert.DeserializeObject<SpotifyPlayerState>(contentStr);

        if (playerState == null)
            return null;

        var now = _clock.Now;

        stopwatch.Stop();
        var latency = stopwatch.Elapsed;

        playerState.RetrievedAt = ((DateTimeOffset)(now - latency)).ToUnixTimeSeconds();

        return playerState;
    }

    public async Task<bool> SkipTrackAsync(string? device_id, CancellationToken stoppingToken = default)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri($"{SpotifyEndPoint}/me/player/next?device_id={device_id}"),
            Method = HttpMethod.Post
        };

        var response = await _httpClient.SendAsync(request, stoppingToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<SpotifyPlayerQueue> GetPlayerQueueAsync(CancellationToken stoppingToken = default)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri($"{SpotifyEndPoint}/me/player/queue")
        };

        var response = await _httpClient.SendAsync(request, stoppingToken);

        string contentStr = await response.Content.ReadAsStringAsync(stoppingToken);

        return JsonConvert.DeserializeObject<SpotifyPlayerQueue>(contentStr)!;
    }

    public async Task<bool> AddTrackInQueueAsync(string trackId, string? device_id, CancellationToken stoppingToken = default)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri($"{SpotifyEndPoint}/me/player/queue?uri=spotify:track:{trackId}&device_id={device_id}"),
            Method = HttpMethod.Post
        };

        var response = await _httpClient.SendAsync(request, stoppingToken);

        return response.IsSuccessStatusCode;
    }

    private async void GetAuthorizationToken()
    {
        var tokenData = await _spotifyTokenStoreService.GetTokenAsync();

        if (tokenData == null)
            return;

        UpdateAuthorizationHeader(tokenData.AccessToken);
    }

    private void UpdateAuthorizationHeader(string accessToken)
    {
        if (_httpClient.DefaultRequestHeaders.Authorization != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
    }

    private void AccessTokenChanged(object? sender, AccessTokenChangedEventArgs args)
    {
        _logger.LogDebug("Access token changed: {accessToken}", args.AccessToken);
        UpdateAuthorizationHeader(args.AccessToken);
    }

    public void Dispose()
    {
        _spotifyTokenStoreService.AccessTokenChanged -= AccessTokenChanged;
    }
}