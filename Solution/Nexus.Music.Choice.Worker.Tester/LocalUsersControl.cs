using Nexus.Music.Choice.Worker.Conector.Enums;

namespace Nexus.Music.Choice.Worker.Tester;

internal interface ILocalUsersControl
{
    public void ChangeUsersConnection(IEnumerable<Guid> usersId, ConnectionNotifyType connectionState);
    public Guid? RandomUserId();
}

internal class LocalUsersControl : ILocalUsersControl
{
    private readonly HashSet<Guid> _connectedUsers = [];

    public void ChangeUsersConnection(IEnumerable<Guid> usersId, ConnectionNotifyType connectionState)
    {
        foreach (var userId in usersId)
        {
            if (connectionState == ConnectionNotifyType.Connect)
            {
                _connectedUsers.Add(userId);
            }
            else if (connectionState == ConnectionNotifyType.Disconnect)
            {
                _connectedUsers.Remove(userId);
            }
        }
    }

    public Guid? RandomUserId()
    {
        if (_connectedUsers.Count == 0)
            return null;

        var random = new Random();
        return _connectedUsers.ElementAt(random.Next(_connectedUsers.Count));
    }
}