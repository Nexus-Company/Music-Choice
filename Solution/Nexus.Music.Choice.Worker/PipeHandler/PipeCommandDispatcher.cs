using Nexus.Music.Choice.Worker.Base.Dispatcher;
using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Interfaces;

namespace Nexus.Music.Choice.Worker.PipeHandler;

internal class PipeCommandDispatcher : BaseCommandDispatcher<PipeReader>, ICommandDispatcher<PipeReader>
{
    public event EventHandler<Command> CommandReceived;

    public PipeCommandDispatcher(ILogger<PipeCommandDispatcher> logger, IInteractionService interactionService)
        : base(logger, interactionService)
    {
    }
}