﻿using Nexus.Music.Choice.Worker.Services;
using Nexus.Music.Choice.Worker.Workers.Handlers;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Nexus.Music.Choice.Worker.Workers.MusicPipe;

internal class MusicPipeWriter : IPipeStream
{
    private readonly StreamWriter _writer;
    private readonly IInteractionService _interactionService;
    private readonly ILogger<MusicPipeConnectionHandler> _logger;
    private readonly Thread _task;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ConcurrentQueue<object> _eventQueue;

    public MusicPipeWriter(StreamWriter writer, IInteractionService interactionService, ILogger<MusicPipeConnectionHandler> logger)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _interactionService = interactionService ?? throw new ArgumentNullException(nameof(interactionService));
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
        _logger.LogInformation("Music Pipe Writer stopped successfully!");
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
                    await _writer.WriteLineAsync(json);
                    await _writer.FlushAsync();
                    _logger.LogInformation("Interaction Event written to pipe: {interaction}", evt);
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
}
