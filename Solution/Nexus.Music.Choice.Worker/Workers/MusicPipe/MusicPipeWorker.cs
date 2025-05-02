using Nexus.Music.Choice.Worker.Workers.Handlers;
using System.IO.Pipes;

namespace Nexus.Music.Choice.Worker.Workers.MusicPipe;

public class MusicPipeWorker : BackgroundService
{
    private const string PipeName = "MusicChoicePipe";
    private readonly ILogger<MusicPipeWorker> _logger;
    private readonly IMusicPipeConnectionHandler _connectionHandler;

    public MusicPipeWorker(ILogger<MusicPipeWorker> logger, IMusicPipeConnectionHandler connectionHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionHandler = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Waiting for connections...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var server = CreatePipeServer();
            await server.WaitForConnectionAsync(stoppingToken);

            _connectionHandler.HandleConnection(server, stoppingToken);
        }
    }

    private NamedPipeServerStream CreatePipeServer()
    {
        return new NamedPipeServerStream(
            PipeName,
            PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
    }
}