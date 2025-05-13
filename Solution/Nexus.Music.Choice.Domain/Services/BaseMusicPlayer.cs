using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Enums;
using Nexus.Music.Choice.Domain.Services.EventArgs;

namespace Nexus.Music.Choice.Domain.Services;

public abstract class BaseMusicPlayer<TPlayerState>
    where TPlayerState : PlayerState
{
    protected readonly ILogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public event EventHandler<TrackQueueChangedEventArgs>? TrackQueueChanged;
    public event EventHandler<PlayerStateChangedEventArgs>? PlayerStateChanged;

    protected TPlayerState? _lastPlayerState;
    protected IEnumerable<Track> _userQueue;

#pragma warning disable IDE0052 // Remover membros particulares não lidos
    private readonly Timer _timer;
#pragma warning restore IDE0052 // Remover membros particulares não lidos

    public BaseMusicPlayer(ILogger logger)
    {
        _logger = logger;
        _userQueue = [];

        _timer = new Timer(async _ => await CheckPlayerStateAsync(), null, 0, 500);
    }

    protected abstract Task<IEnumerable<Track>> GetPlayerQueueAsync(CancellationToken cancellationToken = default);
    protected abstract Task<TPlayerState?> GetPlayerCurrentStateAsync(CancellationToken cancellationToken = default);

    private async Task CheckPlayerStateAsync()
    {
        if (!await _semaphore.WaitAsync(0))
            return;

        try
        {
            var currentState = await GetPlayerCurrentStateAsync();

            if (currentState != null && (_lastPlayerState == null || !currentState.Equals(_lastPlayerState)))
            {
                _logger.LogInformation("Player State Changed. New player state: {state}", JsonConvert.SerializeObject(currentState));

                var @event = GetPlayerEvent(_lastPlayerState, currentState);
                var args = new PlayerStateChangedEventArgs(@event, _lastPlayerState, currentState);

                PlayerStateChanged?.Invoke(this, args);
                _lastPlayerState = currentState;
                StartExecuteCheckQueueState();
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

    #region Queue List

    protected void StartExecuteCheckQueueState()
    {
#pragma warning disable CA2016 // Encaminhe o parâmetro 'CancellationToken' para os métodos
        _ = Task.Run(CheckQueueStateAsync); // Execute CheckQueueStateAsync without blocking the current flow  
#pragma warning restore CA2016 // Encaminhe o parâmetro 'CancellationToken' para os métodos
    }

    private async Task CheckQueueStateAsync()
    {
        try
        {
            var currentQueue = await GetPlayerQueueAsync();
            var previousQueue = _userQueue;

            var currentIds = currentQueue.Select(t => t.Id).ToArray();
            var previousIds = previousQueue.Select(t => t.Id).ToArray();

            var addedIds = currentIds.Where(id => !previousIds.Contains(id)).ToArray();
            var removedIds = previousIds.Where(id => !currentIds.Contains(id)).ToArray();

            _userQueue = currentQueue;

            // 1. Added
            var addedHandled = HandleAddedTracks(currentQueue, currentQueue.Where(t => addedIds.Contains(t.Id)));

            // 2. Removed
            var removedHandled = HandleRemovedTracks(previousQueue, removedIds);

            // 3. Reordered (somente se não for uma simples adição ou remoção)
            bool isSingleAddition = addedIds.Length == 1;
            bool isSingleRemoval = removedIds.Length == 1 && _lastPlayerState?.Item?.Id == removedIds[0];

            if (!addedHandled && !removedHandled && !isSingleAddition && !isSingleRemoval)
            {
                HandleReorderedTracks(previousIds, currentIds, currentQueue);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check queue state.");
        }
    }

    private bool HandleAddedTracks(IEnumerable<Track> currentQueue, IEnumerable<Track> addedTracks)
    {
        var addedList = addedTracks.ToList();
        if (addedList.Count == 0)
            return false;

        var currentList = currentQueue.ToList();
        var lastIndex = currentList.Count - 1;

        // Verifica se todos os tracks adicionados estão no final da fila
        var addedIndexes = addedList
            .Select(track => new
            {
                Track = track,
                Index = currentList.FindIndex(t => t.Id == track.Id)
            })
            .Where(x => x.Index >= 0)
            .ToList();

        // Se qualquer música estiver antes do final, considera reorder
        var totalAdded = addedList.Count;
        var minExpectedIndex = currentList.Count - totalAdded;

        var anyInsertedBeforeEnd = addedIndexes.Any(x => x.Index < minExpectedIndex);
        if (anyInsertedBeforeEnd)
        {
            TrackQueueChanged?.Invoke(this,
                new TrackQueueChangedEventArgs(TrackQueueEvent.Reordered, currentList));
            return true;
        }

        // Todas adicionadas ao final
        TrackQueueChanged?.Invoke(this,
            new TrackQueueChangedEventArgs(TrackQueueEvent.Added, addedList));

        return true;
    }

    private bool HandleRemovedTracks(IEnumerable<Track> previousQueue, IEnumerable<string> removedIds)
    {
        var removedList = removedIds.ToList();
        if (removedList.Count == 0)
            return false;

        if (removedList.Count == 1 && _lastPlayerState?.Item?.Id == removedList.First())
            return false;

        var removedTracks = previousQueue
            .Where(track => removedList.Contains(track.Id))
            .ToList();

        if (removedTracks.Count == 1)
        {
            TrackQueueChanged?.Invoke(this,
                new TrackQueueChangedEventArgs(removedTracks[0].Id, TrackQueueEvent.Removed));
        }
        else
        {
            TrackQueueChanged?.Invoke(this,
                new TrackQueueChangedEventArgs(TrackQueueEvent.Removed, removedTracks));
        }

        return true;
    }

    private bool HandleReorderedTracks(string[] previousIds, string[] currentIds, IEnumerable<Track> currentQueue)
    {
        if (!previousIds.SequenceEqual(currentIds) && previousIds.Length == currentIds.Length)
        {
            TrackQueueChanged?.Invoke(this,
                new TrackQueueChangedEventArgs(TrackQueueEvent.Reordered, currentQueue.ToList()));
            return true;
        }

        return false;
    }
    #endregion
}