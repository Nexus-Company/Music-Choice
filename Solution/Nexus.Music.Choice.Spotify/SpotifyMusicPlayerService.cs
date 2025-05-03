using Microsoft.Extensions.Logging;
using Nexus.Music.Choice.Domain;
using Nexus.Music.Choice.Domain.Models;

namespace Nexus.Music.Choice.Spotify;

public class SpotifyMusicPlayerService : IMusicPlayerService
{
    private readonly ILogger<SpotifyMusicPlayerService> _logger;
    public SpotifyMusicPlayerService(ILogger<SpotifyMusicPlayerService> logger)
    {
        _logger = logger;
    }

    public Task<bool> AddSongAsync(string songId, string? deviceId, CancellationToken? cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PlayerState> GetPlayerStateAsync(CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Track>> GetQueueAsync(CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveSongAsync(string songId, string? deviceId, CancellationToken? cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SkipSongAsync(string? deviceId, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
}