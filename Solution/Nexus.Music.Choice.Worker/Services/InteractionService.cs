using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Worker.Base.Dispatcher;
using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Base.Models.Enums;
using Nexus.Music.Choice.Worker.Entities;
using Nexus.Music.Choice.Worker.Interfaces;
using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Services;

internal class InteractionService : IInteractionService
{
    private readonly ILogger<InteractionService> _logger;
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IInteractionLogService _interactionLogService;
    private readonly IVoteService _voteService;
    private readonly IMessageDispatcher<IStreamWriter> _messageDispatcher;

    public Dictionary<string, Guid> _userTrackAdd = [];

    public InteractionService(
        InteractContext interactContext,
        IMessageDispatcher<IStreamWriter> messageDispatcher,
        IMusicPlayerService musicPlayerService,
        IVoteService voteService,
        ILogger<InteractionService> logger)
    {
        _logger = logger;
        _musicPlayerService = musicPlayerService;
        _voteService = voteService;
        _messageDispatcher = messageDispatcher;

        _musicPlayerService.PlayerStateChanged += PlayerStateChanged;
    }

    public async Task TrackAddAync(string trackId, Guid userId, CancellationToken cancellationToken = default)
    {
        _userTrackAdd.Add(trackId, userId);

        await _musicPlayerService.AddTrackAsync(trackId, string.Empty, cancellationToken);
        await _interactionLogService.LogActionExecutedAsync(ActionExecutedType.TrackQueueAdd, trackId, userId, cancellationToken);

        _messageDispatcher.DispatchMessage(new Message
        {
            MessageType = MessageType.Event,
            EventType = EventType.TrackQueueAdd,
            Data = trackId
        });
    }

    public async Task TrackRemoveAsync(string trackId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (_userTrackAdd.TryGetValue(trackId, out Guid addUserId))
        {
            if (addUserId == userId)
            {
                await _musicPlayerService.RemoveTrackAsync(trackId, string.Empty, cancellationToken);
                await _interactionLogService.LogActionExecutedAsync(ActionExecutedType.TrackQueueRemove, trackId, userId, cancellationToken);
                _userTrackAdd.Remove(trackId);
            }
        }
        else
        {
            _logger.LogWarning("User {userId} tried to remove track {trackId} that was not added by them.", userId, trackId);
            throw new InvalidOperationException("You cannot remove a track that was not added by you.");
        }

        _voteService.AddVote(userId, VotingType.TrackQueueRemove, trackId);
        await _interactionLogService.LogVoteAsync(userId, VotingType.TrackQueueRemove, trackId, cancellationToken);

        bool shouldBeAction = await _voteService.ShouldActionBePerformedAsync(VotingType.TrackQueueRemove, trackId, cancellationToken);

        if (shouldBeAction)
        {
            await _musicPlayerService.RemoveTrackAsync(trackId, string.Empty, cancellationToken);
            await _interactionLogService.LogActionExecutedAsync(ActionExecutedType.TrackQueueRemoveByVote, trackId, null, cancellationToken);

            _messageDispatcher.DispatchMessage(new Message
            {
                MessageType = MessageType.Event,
                EventType = EventType.TrackQueueRemove,
                Data = trackId
            });
            _logger.LogInformation("Remove track {id} action performed.", trackId);
        }
        else
        {
            _logger.LogInformation("Vote remove track {id} action performed.", trackId);
        }
    }

    public async Task VoteSkipAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string trackId = string.Empty;
        _voteService.AddVote(userId, VotingType.SkipTrack);

        await _interactionLogService.LogVoteAsync(userId, VotingType.SkipTrack, trackId, cancellationToken);

        bool shouldBeAction = await _voteService.ShouldActionBePerformedAsync(VotingType.SkipTrack, cancellationToken);

        if (shouldBeAction)
        {
            await _musicPlayerService.SkipTrackAsync(string.Empty, cancellationToken);
            await _interactionLogService.LogActionExecutedAsync(ActionExecutedType.SkipTrackByVote, trackId, null, cancellationToken);

            _messageDispatcher.DispatchMessage(new Message
            {
                MessageType = MessageType.Event,
                EventType = EventType.SkipTrack
            });
            _logger.LogInformation("Skip track {trackId} action performed.", trackId);
        }
        else
        {
            _logger.LogInformation("Vote skip track {trackId} action performed.", trackId);
        }
    }

    private void PlayerStateChanged(object? sender, PlayerStateChangedEventArgs e)
    {
        _messageDispatcher.DispatchMessage(new Message()
        {
            MessageType = MessageType.PlayerState,
            Data = e.NewState
        });
    }
}