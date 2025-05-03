using Nexus.Music.Choice.Domain.Models;

namespace Nexus.Music.Choice.Domain;

public interface IMusicPlayerService
{
    public Task<bool> AddSongAsync(string songId, string? deviceId, CancellationToken? cancellationToken = default);
    public Task<bool> RemoveSongAsync(string songId, string? deviceId, CancellationToken? cancellationToken = default);
    public Task<bool> SkipSongAsync(string? deviceId, CancellationToken? cancellationToken = default);
    public Task<IEnumerable<Track>> GetQueueAsync(CancellationToken? cancellationToken = default);
    public Task<PlayerState> GetPlayerStateAsync(CancellationToken? cancellationToken = default);
}