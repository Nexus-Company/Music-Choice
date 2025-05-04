using Microsoft.Extensions.Logging;
using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Services.Interfaces;

namespace Nexus.Music.Choice.Spotify.Services;

public class SpotifyMusicPlayerService : IMusicPlayerService
{
    private readonly ILogger<SpotifyMusicPlayerService> _logger;
    public event EventHandler<PlayerStateChangedEventArgs> PlayerStateChanged;

    public SpotifyMusicPlayerService(
        ISpotifyApiService apiService,
        ILogger<SpotifyMusicPlayerService> logger)
    {
        _logger = logger;
    }

    public Task<bool> AddTrackAsync(string songId, string? deviceId, CancellationToken? cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PlayerState> GetPlayerStateAsync(CancellationToken? cancellationToken = null)
    {
        PlayerState playerState = new DefaultPlayerState
        {
        };

        return Task.FromResult(playerState);
    }

    public Task<IEnumerable<Track>> GetQueueAsync(CancellationToken? cancellationToken = null)
    {
        IEnumerable<Track> queue = [];

        return Task.FromResult(queue);
    }

    public Task<bool> RemoveTrackAsync(string songId, string? deviceId, CancellationToken? cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SkipTrackAsync(string? deviceId, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
}


public class DefaultPlayerState : PlayerState
{


}