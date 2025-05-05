using Nexus.Music.Choice.Worker.Conector.Base;
using Nexus.Music.Choice.Worker.Conector.Enums;
using System.Text.Json;

namespace Nexus.Music.Choice.Worker.Conector.Messages;

internal class ConnectionNotifyMessage : BaseMessage
{
    public IEnumerable<Guid> UsersId { get; set; }
    public ConnectionNotifyType NotifyType { get; set; }

    public ConnectionNotifyMessage(IEnumerable<Guid> usersId, ConnectionNotifyType notifyType)
    {
        UsersId = usersId;
        NotifyType = notifyType;
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
                NotifyType
            }
        }, JsonSerializerOptions);
    }
}