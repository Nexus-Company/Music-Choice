using Nexus.Music.Choice.Worker.Base.Dispatcher;
using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.PipeHandler;

internal class PipeCommandDispatcher : BaseCommandDispatcher<PipeReader>, ICommandDispatcher<PipeReader>
{
    public event EventHandler<Command> CommandReceived;

    public PipeCommandDispatcher(ILogger<PipeCommandDispatcher> logger, IFeedbackService feedBackService, IInteractionService interactionService)
        : base(logger, feedBackService, interactionService)
    {
    }
}