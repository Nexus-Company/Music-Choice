using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Enums;
using Nexus.Music.Choice.Domain.Services.EventArgs;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Models;
using Nexus.Music.Choice.Spotify.Services.Interfaces;

namespace Nexus.Music.Choice.Spotify.Services;

public class SpotifyMusicPlayerService : IMusicPlayerService
{
    private readonly ILogger<SpotifyMusicPlayerService> _logger;
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public event EventHandler<TrackQueueChangedEventArgs>? TrackQueueChanged;
    public event EventHandler<PlayerStateChangedEventArgs>? PlayerStateChanged;

    private SpotifyPlayerState? _lastPlayerState;
    private IEnumerable<SpotifyTrack> _userQueue;

#pragma warning disable IDE0052 // Remover membros particulares não lidos
    private readonly Timer _timer;
#pragma warning restore IDE0052 // Remover membros particulares não lidos

    public PlayerState? PlayerState => _lastPlayerState;

    public SpotifyMusicPlayerService(
        ISpotifyApiService apiService,
        ILogger<SpotifyMusicPlayerService> logger)
    {
        _logger = logger;
        _spotifyApiService = apiService;
        _userQueue = [];

        _timer = new Timer(async _ => await CheckPlayerStateAsync(), null, 0, 500);
    }

    public async Task<bool> AddTrackAsync(string trackId, CancellationToken cancellationToken = default)
    {
        await WaitForPlayerStateAsync(cancellationToken);

        if (_lastPlayerState?.IsPlaying == true)
        {
            var result = await _spotifyApiService.AddTrackInQueueAsync(trackId, _lastPlayerState.Device?.Id, cancellationToken);

            if (result)
                _logger.LogInformation("Track added to queue successfully.");
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

    private async Task CheckPlayerStateAsync()
    {
        if (!await _semaphore.WaitAsync(0))
            return;

        try
        {
            var currentState = await _spotifyApiService.GetPlayerStateAsync();

            if (currentState != null && (_lastPlayerState == null || !currentState.Equals(_lastPlayerState)))
            {
                _logger.LogInformation("Player State Changed. New player state: {state}", JsonConvert.SerializeObject(currentState));

                var queue = await _spotifyApiService.GetPlayerQueueAsync();
                _userQueue = queue.Queue;

                var @event = GetPlayerEvent(_lastPlayerState, currentState);
                var args = new PlayerStateChangedEventArgs(@event, _lastPlayerState, currentState);
                PlayerStateChanged?.Invoke(this, args);
                _lastPlayerState = currentState;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check player state.");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static PlayerEvent GetPlayerEvent(PlayerState? oldPlayerState, PlayerState newPlayerState)
    {
        if ((oldPlayerState == null || !oldPlayerState.IsPlaying) && newPlayerState.IsPlaying)
            return PlayerEvent.Play;

        if ((oldPlayerState?.IsPlaying ?? false) && !newPlayerState.IsPlaying)
            return PlayerEvent.Pause;

        if (oldPlayerState?.Item?.Id != newPlayerState.Item?.Id)
            return PlayerEvent.Next;

        if (oldPlayerState?.Volume != newPlayerState.Volume)
        {
            if (oldPlayerState?.Volume <= 0 && newPlayerState.Volume > 0)
                return PlayerEvent.Unmute;

            if (oldPlayerState?.Volume > 0 && newPlayerState.Volume <= 0)
                return PlayerEvent.Mute;

            return PlayerEvent.VolumeChange;
        }

        return PlayerEvent.Play;
    }

    private async Task WaitForPlayerStateAsync(CancellationToken cancellationToken)
    {
        while (_lastPlayerState == null)
        {
            await Task.Delay(200, cancellationToken);
        }
    }
}