using Nexus.Music.Choice.Worker.Base.Dispatcher;
using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Interfaces;

namespace Nexus.Music.Choice.Worker.PipeHandler;

internal class PipeEventDispatcher : BaseDispatcher<PipeWriter>, IEventDispatcher<PipeWriter>
{
    public PipeEventDispatcher(ILogger<PipeEventDispatcher> logger)
        : base(logger)
    {
    }

    public void DispatchEvent(Event @event)
    {
        foreach (var stream in _streams.Values)
        {
            lock (stream)
            {
                ((IStreamWriter)stream).AddToSendQueue(@event);
            }
        }
    }
}