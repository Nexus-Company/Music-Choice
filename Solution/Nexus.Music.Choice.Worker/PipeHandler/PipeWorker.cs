using System.IO.Pipes;

namespace Nexus.Music.Choice.Worker.PipeHandler;

public class PipeWorker : BackgroundService
{
    private const string PipeName = "MusicChoicePipe";
    private readonly ILogger<PipeWorker> _logger;
    private readonly IPipeConnectionHandler _connectionHandler;

    public PipeWorker(ILogger<PipeWorker> logger, IPipeConnectionHandler connectionHandler)
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

            try
            {
                await server.WaitForConnectionAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

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