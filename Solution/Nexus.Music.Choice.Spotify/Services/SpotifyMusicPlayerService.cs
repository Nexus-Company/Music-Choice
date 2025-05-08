using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Models;
using Nexus.Music.Choice.Spotify.Services.Interfaces;

namespace Nexus.Music.Choice.Spotify.Services;

public class SpotifyMusicPlayerService : IMusicPlayerService
{
    private readonly ILogger<SpotifyMusicPlayerService> _logger;
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    public event EventHandler<PlayerStateChangedEventArgs>? PlayerStateChanged;
    private SpotifyPlayerState? _lastPlayerState;
    private IEnumerable<SpotifyTrack> _userQueue;
#pragma warning disable IDE0052 // Remover membros particulares não lidos
    private readonly Timer _timer;

    public PlayerState? PlayerState { get => _lastPlayerState; }
#pragma warning restore IDE0052 // Remover membros particulares não lidos
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
        if (_lastPlayerState == null)
        {
            while (_lastPlayerState == null)
            {
                await Task.Delay(100, cancellationToken);
            }
        }

        if (_lastPlayerState?.IsPlaying == true)
        {
            var result = await _spotifyApiService.AddTrackInQueueAsync(trackId, _lastPlayerState.Device?.Id, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Track skipped successfully.");
            }
            else
            {
                _logger.LogWarning("Failed to skip track.");
            }

            return result;
        }

        return false;
    }

    public async Task<PlayerState> GetPlayerStateAsync(CancellationToken cancellationToken = default)
    {
        if (_lastPlayerState == null)
        {
            while (_lastPlayerState == null)
            {
                await Task.Delay(100, cancellationToken);
            }
        }

        return _lastPlayerState;
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
        if (_lastPlayerState == null)
        {
            while (_lastPlayerState == null)
            {
                await Task.Delay(100, cancellationToken);
            }
        }

        if (_lastPlayerState?.IsPlaying == true)
        {
            var result = await _spotifyApiService.SkipTrackAsync(_lastPlayerState.Device?.Id, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Track skipped successfully.");
            }
            else
            {
                _logger.LogWarning("Failed to skip track.");
            }

            return result;
        }

        return false;
    }

    private async Task CheckPlayerStateAsync()
    {
        if (!await _semaphore.WaitAsync(0))
        {
            // Outra execução já está rodando → ignorar esta chamada
            return;
        }

        try
        {
            var currentState = await _spotifyApiService.GetPlayerStateAsync();

            if (currentState != null && (_lastPlayerState == null || !(currentState?.Equals(_lastPlayerState) ?? false)))
            {
                _logger.LogInformation("Player State Chagend new player State is: {state}", JsonConvert.SerializeObject(currentState));

                var queue = await _spotifyApiService.GetPlayerQueueAsync();

                _userQueue = queue.Queue;

                var args = new PlayerStateChangedEventArgs(_lastPlayerState, currentState);
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
}