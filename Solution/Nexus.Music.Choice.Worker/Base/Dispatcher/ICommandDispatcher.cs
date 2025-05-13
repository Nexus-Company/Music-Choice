using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Interfaces;

namespace Nexus.Music.Choice.Worker.Base.Dispatcher;

public interface IDispatcher<out T>
    where T : class, IStream
{
    public void RegisterStream(int connectionId, IStream stream);
    public void UnregisterStream(int connectionId);
}

public interface ICommandDispatcher<out T> : IDispatcher<T>
    where T : class, IStreamReader
{
    public void DispatchCommand(object? sender, Command command);
}

internal interface IMessageDispatcher<out T> : IDispatcher<T>
    where T : class, IStreamWriter
{
    public void DispatchMessage(Message message);
}