using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Base.Models.Enums;
using Nexus.Music.Choice.Worker.Interfaces;
using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Base.Dispatcher;

public abstract class BaseCommandDispatcher<T> : BaseDispatcher<T>, ICommandDispatcher<T>
    where T : class, IStreamReader
{
    private protected readonly IInteractionService _interactionService;
    private protected readonly IFeedbackService _feedBackService;

    protected BaseCommandDispatcher(ILogger logger, IFeedbackService feedBackService, IInteractionService interactionService) : base(logger)
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

    }

    public async Task ProcessQueueChange(Guid userId, object? data)
    {

    }
}