using Microsoft.Extensions.Logging;
using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Models;
using Nexus.Music.Choice.Spotify.Services.Interfaces;

namespace Nexus.Music.Choice.Spotify.Services;

public class SpotifyMusicPlayerService : BaseMusicPlayer<SpotifyPlayerState>, IMusicPlayerService
{
    private readonly ISpotifyApiService _spotifyApiService;

    public PlayerState? PlayerState => _lastPlayerState;

    public SpotifyMusicPlayerService(
        ISpotifyApiService apiService,
        ILogger<SpotifyMusicPlayerService> logger)
        : base(logger)
    {
        _spotifyApiService = apiService;
    }

    public async Task<bool> AddTrackAsync(string trackId, CancellationToken cancellationToken = default)
    {
        await WaitForPlayerStateAsync(cancellationToken);

        if (_lastPlayerState?.IsPlaying == true)
        {
            var result = await _spotifyApiService.AddTrackInQueueAsync(trackId, _lastPlayerState.Device?.Id, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Track added to queue successfully.");
                StartExecuteCheckQueueState();
            }
            else
                _logger.LogWarning("Failed to add track to queue.");

            return result;
        }

        return false;
    }

    public async Task<PlayerState> GetPlayerStateAsync(CancellationToken cancellationToken = default)
    {
        await WaitForPlayerStateAsync(cancellationToken);
        return _lastPlayerState!;
    }

    public Task<IEnumerable<Track>> GetQueueAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_userQueue.Select(trk => (Track)trk));
    }

    public Task<bool> RemoveTrackAsync(string songId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> SkipTrackAsync(CancellationToken cancellationToken = default)
    {
        await WaitForPlayerStateAsync(cancellationToken);

        if (_lastPlayerState?.IsPlaying == true)
        {
            var result = await _spotifyApiService.SkipTrackAsync(_lastPlayerState.Device?.Id, cancellationToken);

            if (result)
                _logger.LogInformation("Track skipped successfully.");
            else
                _logger.LogWarning("Failed to skip track.");

            return result;
        }

        return false;
    }

    private async Task WaitForPlayerStateAsync(CancellationToken cancellationToken)
    {
        while (_lastPlayerState == null)
        {
            await Task.Delay(200, cancellationToken);
        }
    }

    protected override async Task<IEnumerable<Track>> GetPlayerQueueAsync(CancellationToken cancellationToken = default)
    {
        var spotifyQueue = await _spotifyApiService.GetPlayerQueueAsync(cancellationToken);

        return spotifyQueue.Queue;
    }

    protected override async Task<SpotifyPlayerState?> GetPlayerCurrentStateAsync(CancellationToken cancellationToken = default)
    {
        return await _spotifyApiService.GetPlayerStateAsync(cancellationToken);
    }
}