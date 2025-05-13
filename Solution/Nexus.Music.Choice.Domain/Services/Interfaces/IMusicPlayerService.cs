using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.EventArgs;

namespace Nexus.Music.Choice.Domain.Services.Interfaces;

public interface IMusicPlayerService
{
    public PlayerState? PlayerState { get; }
    public event EventHandler<PlayerStateChangedEventArgs>? PlayerStateChanged;
    public event EventHandler<TrackQueueChangedEventArgs>? TrackQueueChanged;
    public Task<bool> AddTrackAsync(string songId, CancellationToken cancellationToken = default);
    public Task<bool> RemoveTrackAsync(string songId, CancellationToken cancellationToken = default);
    public Task<bool> SkipTrackAsync(CancellationToken cancellationToken = default);
    public Task<IEnumerable<Track>> GetQueueAsync(CancellationToken cancellationToken = default);
    public Task<PlayerState> GetPlayerStateAsync(CancellationToken cancellationToken = default);
}