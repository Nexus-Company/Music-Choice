using Nexus.Music.Choice.Domain.Models;

namespace Nexus.Music.Choice.Domain.Services.Interfaces;

public interface IMusicPlayerService
{
    public event EventHandler<PlayerStateChangedEventArgs>? PlayerStateChanged;
    public Task<bool> AddTrackAsync(string songId, string? deviceId, CancellationToken cancellationToken = default);
    public Task<bool> RemoveTrackAsync(string songId, string? deviceId, CancellationToken cancellationToken = default);
    public Task<bool> SkipTrackAsync(string? deviceId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Track>> GetQueueAsync(CancellationToken cancellationToken = default);
    public Task<PlayerState> GetPlayerStateAsync(CancellationToken cancellationToken = default);
}

public class PlayerStateChangedEventArgs : EventArgs
{
    public PlayerState? OldState { get; }
    public PlayerState NewState { get; }

    public PlayerStateChangedEventArgs(PlayerState? oldState, PlayerState newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}