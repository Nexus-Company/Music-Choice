using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Models;

namespace Nexus.Music.Choice.Spotify.Services.Interfaces;

public interface ISpotifyApiService : IApiService
{
    public Task<SpotifyPlayerState> GetPlayerStateAsync(CancellationToken stoppingToken = default);
    Task<bool> SkipTrackAsync(string? id, CancellationToken cancellationToken = default);
}

public interface ISpotifyTokenStoreService : ITokenStoreService
{
    internal event EventHandler<AccessTokenChangedEventArgs> AccessTokenChanged;
}

public interface ISpotifyApiAuthenticationService : IApiAuthenticationService
{
}