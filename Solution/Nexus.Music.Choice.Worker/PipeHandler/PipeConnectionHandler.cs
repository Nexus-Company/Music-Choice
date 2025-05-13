using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Worker.Base.Dispatcher;
using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Base.Models.MessageData;
using Nexus.Music.Choice.Worker.Services.Interfaces;
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
    private readonly IMessageDispatcher<PipeWriter> _eventDispatcher;
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IUserConnectionControlService _userControlService;
    private int _connectionCounter = 0;

    private readonly List<Task> _tasks = [];
    private bool _disposed = false;

    public PipeConnectionHandler(
        ILogger<PipeConnectionHandler> logger,
        ICommandDispatcher<PipeReader> commandDispatcher,
        IMusicPlayerService musicPlayerService,
        IUserConnectionControlService userControlService,
        IMessageDispatcher<PipeWriter> eventDispatcher)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _musicPlayerService = musicPlayerService ?? throw new ArgumentNullException(nameof(musicPlayerService));
        _userControlService = userControlService ?? throw new ArgumentNullException(nameof(userControlService));
    }

    public async void HandleConnection(NamedPipeServerStream server, CancellationToken stoppingToken)
    {
        int connectionId = _connectionCounter;
        _connectionCounter++;

        var reader = new PipeReader(new StreamReader(server), connectionId, _logger);
        var writer = new PipeWriter(server, _logger);

        _eventDispatcher.RegisterStream(connectionId, writer);
        _commandDispatcher.RegisterStream(connectionId, reader);

        _logger.LogInformation("Client with connection id #{id} connected.", connectionId);

        await SendInitialPlayerState(writer);

        _tasks.Add(MonitorConnection(server, connectionId, stoppingToken));
    }

    private async Task SendInitialPlayerState(PipeWriter streamWriter)
    {
        var playerState = await _musicPlayerService.GetPlayerStateAsync();
        var queue = await _musicPlayerService.GetQueueAsync();
        int onlineUsers = _userControlService.GetTotalActiveUsers();

        streamWriter.AddToSendQueue(new Message(new InitialMessageData(onlineUsers, playerState, queue)));
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

        _userControlService.DisconnectUsers(connectionId);
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