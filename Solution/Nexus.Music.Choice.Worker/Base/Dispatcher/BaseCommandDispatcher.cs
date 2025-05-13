using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Base.Models.CommandData;
using Nexus.Music.Choice.Worker.Base.Models.Enums;
using Nexus.Music.Choice.Worker.Entities;
using Nexus.Music.Choice.Worker.Interfaces;
using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Base.Dispatcher;

public abstract class BaseCommandDispatcher<T> : BaseDispatcher<T>, ICommandDispatcher<T>
    where T : class, IStreamReader
{
    private protected readonly IInteractionService _interactionService;
    private protected readonly IFeedbackService _feedBackService;
    private protected readonly IUserConnectionControlService _userControlService;

    protected BaseCommandDispatcher(
        ILogger logger,
        IFeedbackService feedBackService,
        IUserConnectionControlService userControlService,
        IInteractionService interactionService)
        : base(logger)
    {
        _interactionService = interactionService;
        _feedBackService = feedBackService;
        _userControlService = userControlService;
    }

    public override void RegisterStream(int connectionId, IStream stream)
    {
        ((T)stream).CommandReceived += DispatchCommand;

        base.RegisterStream(connectionId, stream);
    }

    public override void UnregisterStream(int connectionId)
    {
        _streams[connectionId].CommandReceived -= DispatchCommand;

        base.UnregisterStream(connectionId);
    }

    public async void DispatchCommand(object? sender, Command command)
    {
        try
        {
            switch (command.ActionType)
            {
                case ActionType.TrackFeedback:
                    await ProcessTrackFeedback(command);
                    break;
                case ActionType.Vote:
                    await ProcessVote(command);
                    break;
                case ActionType.ConnectionNotify:
                    ProcessConnectionNotify((int)sender!, command);
                    break;
                case ActionType.QueueChange:
                    await ProcessQueueChange(command);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command: {Command}", command);
        }
    }

    private void ProcessConnectionNotify(int connectionId, Command command)
    {
        var connectionNotifyData = command.GetCommandData<ConnectionNotifyCommandData>();

        switch (connectionNotifyData.State)
        {
            case ConnectionState.Connect:
                _userControlService.ConnectUsers(connectionId, connectionNotifyData.UsersId);
                break;
            case ConnectionState.Disconnect:
                _userControlService.DisconnectUsers(connectionNotifyData.UsersId);
                break;
        }
    }

    public async Task ProcessTrackFeedback(Command command)
    {
        var feedbackData = command.GetCommandData<UserTrackFeedbackCommandData>();

        switch (feedbackData.Type)
        {
            case FeedbackType.Like:
                await _feedBackService.TrackLikeAsync(feedbackData.UserId, feedbackData.TrackId);
                break;
            case FeedbackType.Dislike:
                await _feedBackService.TrackDislikeAsync(feedbackData.UserId, feedbackData.TrackId);
                break;
            default:
                _logger.LogWarning("FeedBack type {voteType} is not supported.", feedbackData.Type);
                throw new ArgumentException($"Vote type {feedbackData.Type} is not supported.");
        }
    }

    public async Task ProcessVote(Command command)
    {
        var voteData = command.GetCommandData<VotingCommandData>();

        switch (voteData.Type)
        {
            case VotingType.SkipTrack:
                await _interactionService.VoteSkipAsync(voteData.UserId);
                break;
            case VotingType.TrackQueueRemove:
                if (voteData.TrackId == null)
                    throw new ArgumentException("Track id is null.");

                await _interactionService.TrackRemoveAsync(voteData.TrackId, voteData.UserId);
                break;
            default:
                _logger.LogWarning("Vote type {voteType} is not supported.", voteData.Type);
                throw new ArgumentException($"Vote type {voteData.Type} is not supported.");
        }
    }

    public async Task ProcessQueueChange(Command command)
    {
        var queueChangeData = command.GetCommandData<QueueChangeCommandData>();

        await _interactionService.TrackAddAync(queueChangeData.TrackId, queueChangeData.UserId);
    }
}