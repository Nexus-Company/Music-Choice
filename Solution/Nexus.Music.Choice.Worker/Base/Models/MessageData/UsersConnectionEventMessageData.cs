using Nexus.Music.Choice.Worker.Base.Models.CommandData;
using Nexus.Music.Choice.Worker.Base.Models.Enums;
using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Base.Models.MessageData;

internal class UsersConnectionEventMessageData : Message.IMessageData
{
    public MessageType InternalType => MessageType.UserConnectionEvent;
    public int OnlineUsers { get; set; }
    public IEnumerable<Guid> UsersId { get; private set; }
    public ConnectionState State { get; private set; }
    public string? Cause { get; set; }

    public UsersConnectionEventMessageData(UserConnectionChangedEventArgs args)
    {
        OnlineUsers = args.OnlineUsers;
        UsersId = args.UsersId;
        State = args.State;
        Cause = args.Cause;
    }
}