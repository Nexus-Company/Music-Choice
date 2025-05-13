using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Worker.Base.Models.Enums;

namespace Nexus.Music.Choice.Worker.Base.Models.MessageData;

internal class InitialMessageData : Message.IMessageData
{
    public MessageType InternalType => MessageType.InitialMessage;
    public PlayerState? PlayerState { get; private set; }
    public IEnumerable<Track> Queue { get; private set; }
    public int OnlineUsers { get; set; }

    public InitialMessageData(int onlineUsers, PlayerState? playerState, IEnumerable<Track> queue)
    {
        PlayerState = playerState;
        Queue = queue;
        OnlineUsers = onlineUsers;
    }
}