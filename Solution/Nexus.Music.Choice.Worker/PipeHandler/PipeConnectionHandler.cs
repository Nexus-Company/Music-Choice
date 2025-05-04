using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Worker.Base.Dispatcher;
using Nexus.Music.Choice.Worker.Base.Models;
using System.IO.Pipes;

namespace Nexus.Music.Choice.Worker.PipeHandler;

public interface IPipeConnectionHandler
{
    void HandleConnection(NamedPipeServerStream server, CancellationToken stoppingToken);
}

internal class PipeConnectionHandler : IPipeConnectionHandler, IDisposable
{
    private readonly ILogger<PipeConnectionHandler> _logger;
    private readonly ICommandDispatcher<PipeReader> _commandDispatcher;
    private readonly IEventDispatcher<PipeWriter> _eventDispatcher;
    private readonly IMusicPlayerService _musicPlayerService;
    private int _connectionCounter = 0;

    private readonly List<Task> _tasks = [];
    private bool _disposed = false;

    public PipeConnectionHandler(
        ILogger<PipeConnectionHandler> logger,
        ICommandDispatcher<PipeReader> commandDispatcher,
        IMusicPlayerService musicPlayerService,
        IEventDispatcher<PipeWriter> eventDispatcher)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _musicPlayerService = musicPlayerService?? throw new ArgumentNullException(nameof(musicPlayerService));
    }

    public async void HandleConnection(NamedPipeServerStream server, CancellationToken stoppingToken)
    {
        int connectionId = _connectionCounter;
        _connectionCounter++;

        var reader = new PipeReader(new StreamReader(server), _logger);
        var writer = new PipeWriter(new StreamWriter(server) { AutoFlush = true }, _logger);

        _eventDispatcher.RegisterStream(connectionId, writer);
        _commandDispatcher.RegisterStream(connectionId, reader);

        _logger.LogInformation("Client with connection id #{id} connected.", connectionId);

        await SendServerState(writer);

        _tasks.Add(MonitorConnection(server, connectionId, stoppingToken));
    }

    private async Task SendServerState(PipeWriter streamWriter)
    {
        streamWriter.AddToSendQueue(new Event()
        {
            MessageType = MessageType.PlayerState,
            Data = await _musicPlayerService.GetPlayerStateAsync()
        });

        streamWriter.AddToSendQueue(new Event()
        {
            MessageType = MessageType.TrackQueue,
            Data = await _musicPlayerService.GetQueueAsync()
        });
    }

    private async Task MonitorConnection(
        NamedPipeServerStream server,
        int connectionId,
        CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested && server.IsConnected)
        {
            await Task.Delay(100, stoppingToken);
        }

        if (server.IsConnected)
        {
            server.Disconnect();
        }

        _logger.LogInformation("Client with connection id #{id} disconnected.", connectionId);

        _commandDispatcher.UnregisterStream(connectionId);
        _eventDispatcher.UnregisterStream(connectionId);

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