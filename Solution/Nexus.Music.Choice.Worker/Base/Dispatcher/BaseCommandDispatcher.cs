using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Base.Models.Enums;
using Nexus.Music.Choice.Worker.Interfaces;

namespace Nexus.Music.Choice.Worker.Base.Dispatcher;

public abstract class BaseCommandDispatcher<T> : BaseDispatcher<T>, ICommandDispatcher<T>
    where T : class, IStreamReader
{
    private protected readonly IInteractionService _interactionService;

    protected BaseCommandDispatcher(ILogger logger, IInteractionService interactionService) : base(logger)
    {
        _interactionService = interactionService;
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
        switch (command.ActionType)
        {
            case ActionType.TrackAdd:
                break;
            case ActionType.TrackRemove:
                break;
            case ActionType.TrackLike:
                break;
            case ActionType.TrackDislike:
                break;
            case ActionType.VoteSkip:
                await _interactionService.VoteSkipAsync(command.UserId);
                break;
            default:
                break;
        }
    }
}