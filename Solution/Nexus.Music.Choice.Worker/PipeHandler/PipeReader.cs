using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nexus.Music.Choice.Worker.PipeHandler;

public class PipeReader : IStreamReader
{
    private readonly StreamReader _reader;
    private readonly Thread _task;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly int _connectionId;

    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public event EventHandler<Command>? CommandReceived;

    public PipeReader(StreamReader reader, int connectionId, ILogger logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _logger = logger;
        _task = new Thread(ReadFromPipe);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Start()
    {
        _task.Start();
        _logger.LogDebug("Music Pipe Reader started with success!");
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _logger.LogDebug("Music Pipe Reader stopped with success!");
    }

    private async void ReadFromPipe()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await _reader.BaseStream.ReadAsync(buffer, _cancellationTokenSource.Token);

                if (bytesRead > 0)
                {
                    string text = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    _logger.LogInformation("New message Received: {message}", text);
                    var command = JsonSerializer.Deserialize<Command>(text, jsonOptions);

                    if (command == null)
                        continue;

                    CommandReceived?.Invoke(_connectionId, command!);
                }
                else
                {
                    await Task.Delay(10, _cancellationTokenSource.Token);
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

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _task.Join();
        _reader.Dispose();
        _cancellationTokenSource.Dispose();

        GC.SuppressFinalize(this);
    }
}