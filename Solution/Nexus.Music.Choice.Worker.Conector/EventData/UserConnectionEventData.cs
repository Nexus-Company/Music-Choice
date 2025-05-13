using Nexus.Music.Choice.Worker.Conector.Base;

namespace Nexus.Music.Choice.Worker.Conector.EventData;

public class UserConnectionEventData : IEventData
{
    public int OnlineUsers { get; set; }
    public IEnumerable<Guid> UsersId { get; set; }
    public ConnectionState State { get; set; }
    public string? Cause { get; set; }
}

public enum ConnectionState
{
    Connect,
    Disconnect,
}