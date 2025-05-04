using Microsoft.Extensions.Logging;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Services.Interfaces;

namespace Nexus.Music.Choice.Spotify.Services;

internal class SpotifyApiService : ISpotifyApiService, IDisposable
{
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
    }

    public void AccessTokenChanged(object? sender, AccessTokenChangedEventArgs args)
    {
        _logger.LogDebug("Access token changed: {accessToken}", args.AccessToken);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Barear {args.AccessToken}");
    }

    public void Dispose()
    {
        _spotifyTokenStoreService.AccessTokenChanged -= AccessTokenChanged;
    }
}