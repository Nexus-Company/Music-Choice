namespace Nexus.Music.Choice.Worker.Base.Models.CommandData;

internal class ConnectionNotifyCommandData : ICommandData
{
    public IEnumerable<Guid> UsersId { get; set; }
    public ConnectionState State { get; set; }
}

public enum ConnectionState
{
    Connect,
    Disconnect
}