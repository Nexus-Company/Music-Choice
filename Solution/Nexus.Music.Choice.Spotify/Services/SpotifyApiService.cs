using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Models;
using Nexus.Music.Choice.Spotify.Services.Interfaces;
using System.Net;

namespace Nexus.Music.Choice.Spotify.Services;

internal class SpotifyApiService : ISpotifyApiService, IDisposable
{
    private const string SpotifyEndPoint = "https://api.spotify.com/v1";

    private readonly ISpotifyTokenStoreService _spotifyTokenStoreService;
    private readonly ILogger<SpotifyApiService> _logger;
    private readonly HttpClient _httpClient;

    public SpotifyApiService(
        ISpotifyTokenStoreService authenticationService,
        HttpClient httpClient,
        ILogger<SpotifyApiService> logger)
    {
        _spotifyTokenStoreService = authenticationService;
        _spotifyTokenStoreService.AccessTokenChanged += AccessTokenChanged;
        _httpClient = httpClient;
        _logger = logger;

        GetAuthorizationToken();
    }

    public async Task<SpotifyPlayerState> GetPlayerStateAsync(CancellationToken stoppingToken = default)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri($"{SpotifyEndPoint}/me/player")
        };

        var response = await _httpClient.SendAsync(request, stoppingToken);

        string contentStr = await response.Content.ReadAsStringAsync(stoppingToken);

        return JsonConvert.DeserializeObject<SpotifyPlayerState>(contentStr)!;
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