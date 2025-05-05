using Nexus.Music.Choice.Worker.Interfaces;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nexus.Music.Choice.Worker.PipeHandler;

internal class PipeWriter : IStream, IStreamWriter, IDisposable
{
    private readonly Stream _stream;
    private readonly ILogger<PipeConnectionHandler> _logger;
    private readonly Thread _task;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ConcurrentQueue<object> _eventQueue;

    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public PipeWriter(Stream stream, ILogger<PipeConnectionHandler> logger)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
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
                    var json = JsonSerializer.Serialize(evt, jsonOptions);
                    var bytes = Encoding.UTF8.GetBytes(json + "\n"); // Delimitador para mensagens

                    await _stream.WriteAsync(bytes, _cancellationTokenSource.Token);
                    await _stream.FlushAsync(_cancellationTokenSource.Token);

                    _logger.LogTrace("Message sent to connection (bytes): {message}", json);
                }
                else
                {
                    await Task.Delay(10, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                break; // Graceful exit
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while writing bytes to pipe.");
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _task.Join();
        _stream.Dispose();
        _cancellationTokenSource.Dispose();

        GC.SuppressFinalize(this);
    }

    public void AddToSendQueue(object obj)
    {
        _eventQueue.Enqueue(obj);
    }
}