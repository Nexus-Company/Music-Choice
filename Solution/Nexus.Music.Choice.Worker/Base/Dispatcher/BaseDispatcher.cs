using Nexus.Music.Choice.Worker.Interfaces;

namespace Nexus.Music.Choice.Worker.Base.Dispatcher;

public abstract class BaseDispatcher<T>
    where T : class, IStream
{
    private protected readonly Dictionary<int, T> _streams;
    private protected readonly ILogger _logger;
    private readonly Lock _lock = new();

    public BaseDispatcher(ILogger logger)
    {
        _streams = [];
        _logger = logger;
    }

    public virtual void RegisterStream(int connectionId, IStream stream)
    {
        lock (_lock)
        {
            stream.Start();
            _streams.Add(connectionId, (T)stream);
            _logger.LogDebug("New PipeWriter registered for connection_id {id}", connectionId);
        }
    }

    public virtual void UnregisterStream(int connectionId)
    {
        lock (_lock)
        {
            _streams[connectionId].Stop();
            _streams[connectionId].Dispose();
            _streams.Remove(connectionId);
            _logger.LogDebug("PipeStream unregistered for connection_id {id}.", connectionId);
        }
    }
}
