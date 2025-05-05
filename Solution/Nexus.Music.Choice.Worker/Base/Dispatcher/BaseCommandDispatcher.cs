using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Base.Models.Enums;
using Nexus.Music.Choice.Worker.Interfaces;
using Nexus.Music.Choice.Worker.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nexus.Music.Choice.Worker.Base.Dispatcher;

public abstract class BaseCommandDispatcher<T> : BaseDispatcher<T>, ICommandDispatcher<T>
    where T : class, IStreamReader
{
    private protected readonly IInteractionService _interactionService;
    private protected readonly IFeedbackService _feedBackService;

    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected BaseCommandDispatcher(
        ILogger logger,
        IFeedbackService feedBackService,
        IInteractionService interactionService)
        : base(logger)
    {
        _interactionService = interactionService;
        _feedBackService = feedBackService;
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
        if (command.Data == null || command.UserId == Guid.Empty)
        {
            _logger.LogWarning("Command data or UserId is null. Command: {Command}", command);
            return;
        }

        switch (command.ActionType)
        {
            case ActionType.Feedback:
                await ProcessFeedback(command.UserId, command.Data);
                break;
            case ActionType.Vote:
                await ProcessVote(command.UserId, command.Data);
                break;
            case ActionType.QueueChange:
                await ProcessQueueChange(command.UserId, command.Data);
                break;
        }
    }

    public async Task ProcessFeedback(Guid userId, object? data)
    {

    }

    public async Task ProcessVote(Guid userId, object? data)
    {
        var excp = new ArgumentException("The type of data in message is incorrect.");

        if (data == null || data is not JsonElement jsonElement)
            throw excp;

        var voteData = jsonElement.Deserialize<VoteData>(jsonOptions) ?? throw excp;

        switch (voteData.Type)
        {
            case VotingType.SkipTrack:
                await _interactionService.VoteSkipAsync(userId);
                break;
            case VotingType.TrackQueueRemove:
                if (voteData.TrackId == null)
                    throw excp;

                await _interactionService.TrackRemoveAsync(voteData.TrackId, userId);
                break;
            default:
                _logger.LogWarning("Vote type {voteType} is not supported.", voteData.Type);
                throw new ArgumentException($"Vote type {voteData.Type} is not supported.");
        }
    }

    public async Task ProcessQueueChange(Guid userId, object? data)
    {

    }
}