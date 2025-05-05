namespace Nexus.Music.Choice.Worker.Services.Interfaces;

public interface IUserConnectionControlService
{
    int GetTotalActiveUsers();
    void ConnectUsers(int connectionId, IEnumerable<Guid> userIds);
    void DisconnectUsers(IEnumerable<Guid> userIds);
    void DisconnectUsers(int connectionId);
}