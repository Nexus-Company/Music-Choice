using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Enums;
using Nexus.Music.Choice.Domain.Services.EventArgs;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Worker.Base.Dispatcher;
using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Base.Models.MessageData;
using Nexus.Music.Choice.Worker.Entities;
using Nexus.Music.Choice.Worker.Interfaces;
using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Services;

internal class InteractionService : IInteractionService
{
    private readonly ILogger<InteractionService> _logger;
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IInteractionLogService _interactionLogService;
    private readonly IUserConnectionControlService _usersConnectionService;
    private readonly IVoteService _voteService;
    private readonly IMessageDispatcher<IStreamWriter> _messageDispatcher;

    public Dictionary<string, Guid> _userTrackAdd = [];

    public InteractionService(
        IMessageDispatcher<IStreamWriter> messageDispatcher,
        IMusicPlayerService musicPlayerService,
        IInteractionLogService interactionLogService,
        IVoteService voteService,
        IUserConnectionControlService usersConnectionService,
        ILogger<InteractionService> logger)
    {
        _logger = logger;
        _musicPlayerService = musicPlayerService;
        _voteService = voteService;
        _messageDispatcher = messageDispatcher;
        _interactionLogService = interactionLogService;
        _usersConnectionService = usersConnectionService;

        _musicPlayerService.PlayerStateChanged += PlayerStateChanged;
        _musicPlayerService.TrackQueueChanged += TrackQueueChanged;
        _usersConnectionService.UsersConnectionChanged += UsersConnectionChanged;
    }

    public async Task TrackAddAync(string trackId, Guid userId, CancellationToken cancellationToken = default)
    {
        var playerState = await CheckPlayerStateAsync(cancellationToken);

        if (!_userTrackAdd.TryAdd(trackId, userId) || playerState.Item?.Id == trackId)
            throw new ArgumentException("Music already added in queue.");

        await _musicPlayerService.AddTrackAsync(trackId, cancellationToken);
        await _interactionLogService.LogActionExecutedAsync(ActionExecutedType.TrackQueueAdd, trackId, userId, cancellationToken);
    }

    public async Task TrackRemoveAsync(string trackId, Guid userId, CancellationToken cancellationToken = default)
    {
        _ = await CheckPlayerStateAsync(cancellationToken);

        if (_userTrackAdd.TryGetValue(trackId, out Guid addUserId))
        {
            if (addUserId == userId)
            {
                await _musicPlayerService.RemoveTrackAsync(trackId, cancellationToken);
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
            await _musicPlayerService.RemoveTrackAsync(trackId, cancellationToken);
            await _interactionLogService.LogActionExecutedAsync(ActionExecutedType.TrackQueueRemoveByVote, trackId, null, cancellationToken);

            _logger.LogInformation("Remove track {id} action performed.", trackId);
        }
        else
        {
            _logger.LogInformation("Vote remove track {id} action performed.", trackId);
        }
    }

    public async Task VoteSkipAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _ = await CheckPlayerStateAsync(cancellationToken);

        string? trackId = _musicPlayerService.PlayerState?.Item?.Id;
        _voteService.AddVote(userId, VotingType.SkipTrack, trackId);

        await _interactionLogService.LogVoteAsync(userId, VotingType.SkipTrack, null, cancellationToken);

        bool shouldBeAction = await _voteService.ShouldActionBePerformedAsync(VotingType.SkipTrack, cancellationToken: cancellationToken);

        if (shouldBeAction)
        {
            await _musicPlayerService.SkipTrackAsync(cancellationToken);
            await _interactionLogService.LogActionExecutedAsync(ActionExecutedType.SkipTrackByVote, trackId, null, cancellationToken);

            _logger.LogInformation("Skip track {trackId} action performed.", trackId);
        }
        else
        {
            _logger.LogInformation("Vote skip track {trackId} action performed.", trackId);
        }
    }

    private async Task<PlayerState> CheckPlayerStateAsync(CancellationToken cancellationToken = default)
    {
        var playerState = await _musicPlayerService.GetPlayerStateAsync(cancellationToken);

        if (playerState == null)
            throw new NullReferenceException();

        return playerState;
    }

    private void UsersConnectionChanged(object? sender, UserConnectionChangedEventArgs args)
    {
        _messageDispatcher.DispatchMessage(new Message(new UsersConnectionEventMessageData(args)));
    }

    private void TrackQueueChanged(object? sender, TrackQueueChangedEventArgs args)
    {
        if (args.EventType == TrackQueueEvent.Added && !string.IsNullOrEmpty(args.TrackId))
        {
            if (_userTrackAdd.TryGetValue(args.TrackId, out Guid userId))
                args.Actor = userId;
        }

        _messageDispatcher.DispatchMessage(new Message(new TrackQueueChangedMessageData(args)));
    }

    private void PlayerStateChanged(object? sender, PlayerStateChangedEventArgs args)
    {
        _ = _userTrackAdd.Remove(args.NewState?.Item?.Id ?? string.Empty);
        _voteService.ResetVotesForAction(VotingType.SkipTrack, args.NewState?.Item?.Id);

        _messageDispatcher.DispatchMessage(new Message(new PlayerStateChangedMessageData(args)));
    }
}