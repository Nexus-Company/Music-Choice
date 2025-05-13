using Nexus.Music.Choice.Worker.Base.Models.CommandData;

namespace Nexus.Music.Choice.Worker.Services.Interfaces;

public interface IUserConnectionControlService
{
    public event EventHandler<UserConnectionChangedEventArgs>? UsersConnectionChanged;

    int GetTotalActiveUsers();
    void ConnectUsers(int connectionId, IEnumerable<Guid> userIds);
    void DisconnectUsers(IEnumerable<Guid> userIds);
    void DisconnectUsers(int connectionId);
}

public class UserConnectionChangedEventArgs : EventArgs
{
    public int OnlineUsers { get; set; }
    public IEnumerable<Guid> UsersId { get; private set; }
    public ConnectionState State { get; private set; }
    public string? Cause { get; set; }

    public UserConnectionChangedEventArgs(IEnumerable<Guid> users, ConnectionState connectionState)
    {
        UsersId = users;
        State = connectionState;
    }
}