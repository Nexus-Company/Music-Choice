using Nexus.Music.Choice.Worker.Interfaces;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Nexus.Music.Choice.Worker.PipeHandler;

internal class PipeWriter : IStream, IStreamWriter, IDisposable
{
    private readonly StreamWriter _writer;
    private readonly ILogger<PipeConnectionHandler> _logger;
    private readonly Thread _task;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ConcurrentQueue<object> _eventQueue;

    public PipeWriter(StreamWriter writer, ILogger<PipeConnectionHandler> logger)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cancellationTokenSource = new CancellationTokenSource();
        _eventQueue = new ConcurrentQueue<object>();
        _task = new Thread(WriteToPipe);
    }

    public void Start()
    {
        _task.Start();
        _logger.LogDebug("Music Pipe Writer started successfully!");
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _logger.LogDebug("Music Pipe Writer stopped successfully!");
    }

    private async void WriteToPipe()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                if (_eventQueue.TryDequeue(out var evt))
                {
                    var json = JsonSerializer.Serialize(evt);
                    await _writer.WriteAsync(json);
                    await _writer.FlushAsync();
                    _logger.LogInformation("Message send to connection: {message}", json);
                }
                else
                {
                    await Task.Delay(100, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful exit on cancellation  
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while writing to pipe.");
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _task.Join();
        _writer.Dispose();
        _cancellationTokenSource.Dispose();

        GC.SuppressFinalize(this);
    }

    public void AddToSendQueue(object obj)
    {
        _eventQueue.Enqueue(obj);
    }
}