using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Models;

namespace Nexus.Music.Choice.Spotify.Services.Interfaces;

public interface ISpotifyApiService : IApiService
{
    Task<bool> AddTrackInQueueAsync(string trackId, string? device_id, CancellationToken stoppingToken = default);
    Task<SpotifyPlayerQueue> GetPlayerQueueAsync(CancellationToken stoppingToken = default);
    Task<SpotifyPlayerState?> GetPlayerStateAsync(CancellationToken stoppingToken = default);
    Task<bool> SkipTrackAsync(string? id, CancellationToken cancellationToken = default);
}

public interface ISpotifyTokenStoreService : ITokenStoreService
{
    internal event EventHandler<AccessTokenChangedEventArgs> AccessTokenChanged;
}

public interface ISpotifyApiAuthenticationService : IApiAuthenticationService
{
}