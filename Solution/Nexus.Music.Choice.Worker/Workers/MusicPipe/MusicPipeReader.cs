using Nexus.Music.Choice.Worker.Models;
using Nexus.Music.Choice.Worker.Services;
using System.Text.Json;

namespace Nexus.Music.Choice.Worker.Workers.MusicPipe;

public class MusicPipeReader : IPipeStream
{
    private readonly StreamReader _reader;
    private readonly Thread _task;
    private readonly ILogger _logger;
    private readonly IInteractionService _interactionService;
    private readonly CancellationTokenSource cancellationTokenSource;

    public MusicPipeReader(StreamReader reader, IInteractionService interactionService, ILogger logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _interactionService = interactionService ?? throw new ArgumentNullException(nameof(interactionService));
        _logger = logger;
        _task = new Thread(ReadFromPipe);
        cancellationTokenSource = new CancellationTokenSource();
    }

    public void Start()
    {
        _task.Start();
        _logger.LogInformation("Music Pipe Reader started with success!");
    }

    public void Stop()
    {
        cancellationTokenSource.Cancel();
        _logger.LogDebug("Music Pipe Reader started with stopped!");
    }

    private async void ReadFromPipe()
    {
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await _reader.BaseStream.ReadAsync(buffer, cancellationTokenSource.Token);

                if (bytesRead > 0)
                {
                    string text = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var evt = JsonSerializer.Deserialize<InteractionEvent>(text);

                    _logger.LogInformation("New Interaction Received: {interaction}", evt);

                    ProcessCommand(evt);
                }
                else
                {
                    await Task.Delay(100, cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful exit on cancellation  
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while reading from pipe.");
            }
        }
    }

    private void ProcessCommand(InteractionEvent? evt)
    {
        if (evt == null)
            return;

        switch (evt.Action)
        {
            case ActionType.TrackAdd:
                _interactionService.TrackAddAync(evt.SongId!, evt.ActorId);
                break;
            case ActionType.TrackRemove:
                _interactionService.TrackRemoveAsync(evt.SongId!, evt.ActorId);
                break;
            default:
                _logger.LogInformation("Unknown action");
                break;
        }
    }

    public void Dispose()
    {
        _reader.Dispose();
        cancellationTokenSource.Dispose();
        _task.Join();

        GC.SuppressFinalize(this);
    }
}