using Nexus.Music.Choice.Worker.Base.Models;

namespace Nexus.Music.Choice.Worker.Interfaces;

public interface IStream : IDisposable
{
    void Start();
    void Stop();
}

public interface IStreamReader : IStream
{
    public event EventHandler<Command> CommandReceived;
}

public interface IStreamWriter : IStream
{
    public void AddToSendQueue(object obj);
}