using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Models;
using Nexus.Music.Choice.Spotify.Services.Interfaces;
using System.Threading;

namespace Nexus.Music.Choice.Spotify.Services;

public class SpotifyMusicPlayerService : IMusicPlayerService
{
    private readonly ILogger<SpotifyMusicPlayerService> _logger;
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    public event EventHandler<PlayerStateChangedEventArgs>? PlayerStateChanged;
    private PlayerState? _lastPlayerState;
    private readonly Timer _timer;
    public SpotifyMusicPlayerService(
        ISpotifyApiService apiService,
        ILogger<SpotifyMusicPlayerService> logger)
    {
        _logger = logger;
        _spotifyApiService = apiService;

        _timer = new Timer(async _ => await CheckPlayerStateAsync(), null, 0, 500);
    }

    public Task<bool> AddTrackAsync(string songId, string? deviceId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PlayerState> GetPlayerStateAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_lastPlayerState ?? new SpotifyPlayerState());
    }

    public Task<IEnumerable<Track>> GetQueueAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<Track> queue = [];

        return Task.FromResult(queue);
    }

    public Task<bool> RemoveTrackAsync(string songId, string? deviceId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SkipTrackAsync(string? deviceId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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

            if (_lastPlayerState == null || !currentState.Equals(_lastPlayerState))
            {
                _logger.LogInformation("Player State Chagend new player State is: {state}", JsonConvert.SerializeObject(currentState));
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