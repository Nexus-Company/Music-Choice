using Newtonsoft.Json;
using Nexus.Music.Choice.Worker.Conector.Base;
using Nexus.Music.Choice.Worker.Conector.EventData;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Text;

namespace Nexus.Music.Choice.Worker.Conector;

public class PipeConector : IDisposable
{
    private readonly NamedPipeClientStream _pipeClient;
    private readonly CancellationTokenSource _cts = new();
    private readonly ConcurrentQueue<string> _sendQueue = new();

    private readonly Thread _readThread;
    private readonly Thread _writeThread;

    public event EventHandler<EventMessage>? MessageReceived;

    private readonly JsonSerializerSettings settings = new()
    {
        Converters =
         [
             new EventMessageConverter()
         ]
    };

    public PipeConector(string? serverName = null)
    {
        _pipeClient = new NamedPipeClientStream(serverName ?? ".", "Music-Choice-Worker", PipeDirection.InOut, PipeOptions.Asynchronous);

        _readThread = new Thread(ReadLoop) { IsBackground = true };
        _writeThread = new Thread(WriteLoop) { IsBackground = true };
    }

    public void Start()
    {
        _pipeClient.Connect();
        _readThread.Start();
        _writeThread.Start();
    }

    public void SendMessage(BaseMessage message)
    {
        string text = message.GetJsonTextMessage();

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentNullException(nameof(message), "Message cannot be null or empty.");

        _sendQueue.Enqueue(text);
    }

    public void SendMessage(string text)
    {
        _sendQueue.Enqueue(text);
    }

    private async void ReadLoop()
    {
        var buffer = new byte[1024];
        var stringBuilder = new StringBuilder();

        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                int bytesRead = await _pipeClient.ReadAsync(buffer, _cts.Token);
                if (bytesRead == 0)
                {
                    await Task.Delay(100, _cts.Token);
                    continue;
                }

                string part = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                stringBuilder.Append(part);

                while (stringBuilder.ToString().Contains('\n'))
                {
                    var fullText = stringBuilder.ToString();
                    int index = fullText.IndexOf('\n');
                    string message = fullText[..index].Trim();
                    stringBuilder.Remove(0, index + 1);

                    var eventMessage = JsonConvert.DeserializeObject<EventMessage>(message, settings);

                    MessageReceived?.Invoke(this, eventMessage);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadLoop Error] {ex.Message}");
                break;
            }
        }
    }

    private async void WriteLoop()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                if (_sendQueue.TryDequeue(out var message))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(message + "\n");

                    await _pipeClient.WriteAsync(bytes, _cts.Token);
                    await _pipeClient.FlushAsync(_cts.Token);
                }
                else
                {
                    await Task.Delay(50, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WriteLoop Error] {ex.Message}");
                break;
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _readThread.Join();
        _writeThread.Join();
        _pipeClient.Dispose();
        _cts.Dispose();

        GC.SuppressFinalize(this);
    }
}