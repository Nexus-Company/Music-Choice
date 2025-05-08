using Nexus.Music.Choice.Worker.Conector.Base;
using Nexus.Music.Choice.Worker.Conector.Enums;
using System.Text.Json;

namespace Nexus.Music.Choice.Worker.Conector.Messages;

public class ConnectionNotifyMessage : BaseMessage
{
    public IEnumerable<Guid> UsersId { get; set; }
    public ConnectionNotifyType State { get; set; }

    public ConnectionNotifyMessage(IEnumerable<Guid> usersId, ConnectionNotifyType state)
    {
        UsersId = usersId;
        State = state;
    }

    public ConnectionNotifyMessage(Guid user, ConnectionNotifyType notifyType)
        : this([user], notifyType)
    {

    }

    internal override string GetJsonTextMessage()
    {
        return JsonSerializer.Serialize(new
        {
            ActionType = "ConnectionNotify",
            Data = new
            {
                UsersId,
                State
            }
        }, JsonSerializerOptions);
    }
}