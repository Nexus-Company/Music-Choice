using Nexus.Music.Choice.Worker.Services;
using Nexus.Music.Choice.Worker.Workers.MusicPipe;
using System.IO.Pipes;

namespace Nexus.Music.Choice.Worker.Workers.Handlers;

public interface IMusicPipeConnectionHandler
{
    void HandleConnection(NamedPipeServerStream server, CancellationToken stoppingToken);
}

public class MusicPipeConnectionHandler : IMusicPipeConnectionHandler, IDisposable
{
    private readonly ILogger<MusicPipeConnectionHandler> _logger;
    private readonly IInteractionService _interactionService;
    private int _connectionCounter = 0;

    private readonly List<Task> _tasks = [];
    private bool _disposed = false;

    public MusicPipeConnectionHandler(ILogger<MusicPipeConnectionHandler> logger, IInteractionService interactionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _interactionService = interactionService ?? throw new ArgumentNullException(nameof(interactionService));
    }

    public void HandleConnection(NamedPipeServerStream server, CancellationToken stoppingToken)
    {
        _connectionCounter++;
        var connectionId = _connectionCounter;

        _logger.LogInformation("New connection with id #{ConnectionId}", connectionId);

        var reader = new StreamReader(server);
        var writer = new StreamWriter(server) { AutoFlush = true };

        var musicPipeReader = new MusicPipeReader(reader, _interactionService, _logger);
        var musicPipeWriter = new MusicPipeWriter(writer, _interactionService, _logger);

        musicPipeReader.Start();
        musicPipeWriter.Start();

        _tasks.Add(MonitorConnection(server, connectionId, musicPipeReader, musicPipeWriter, stoppingToken));
    }

    private async Task MonitorConnection(
        NamedPipeServerStream server,
        int connectionId,
        MusicPipeReader musicPipeReader,
        MusicPipeWriter musicPipeWriter,
        CancellationToken stoppingToken = default)
    {
        while (!stoppingToken.IsCancellationRequested && server.IsConnected)
        {
            await Task.Delay(100, stoppingToken);
        }

        if (server.IsConnected)
        {
            server.Disconnect();
            _logger.LogInformation("Client with connection_id #{ConnectionId} disconnected by server.", connectionId);
        }
        else
        {
            _logger.LogInformation("Client with connection_id #{ConnectionId} disconnected.", connectionId);
        }

        musicPipeReader.Stop();
        musicPipeWriter.Stop();
        _connectionCounter--;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Cancel and wait for all tasks to complete
                Task.WhenAll(_tasks).ConfigureAwait(false);

                // Liberar outros recursos gerenciados, se necessário
            }

            // Liberar recursos não gerenciados, se houver
            _disposed = true;
        }
    }
}
