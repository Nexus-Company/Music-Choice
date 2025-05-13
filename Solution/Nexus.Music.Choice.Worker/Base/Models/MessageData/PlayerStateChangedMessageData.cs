using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Enums;
using Nexus.Music.Choice.Domain.Services.EventArgs;
using Nexus.Music.Choice.Worker.Base.Models.Enums;

namespace Nexus.Music.Choice.Worker.Base.Models.MessageData;

internal class PlayerStateChangedMessageData : Message.IMessageData
{
    public MessageType InternalType => MessageType.PlayerStateChanged;
    public PlayerEvent PlayerEvent { get; private set; }
    public PlayerState? NewPlayerState { get; private set; }
    public PlayerState? OldPlayerState { get; private set; }

    public PlayerStateChangedMessageData(
        PlayerEvent playerEvent,
        PlayerState? oldPlayerState = null,
        PlayerState? newPlayerState = null)
    {
        PlayerEvent = playerEvent;
        NewPlayerState = newPlayerState;
        OldPlayerState = oldPlayerState;
    }

    public PlayerStateChangedMessageData(PlayerStateChangedEventArgs args)
        : this(args.EventType, args.OldState, args.NewState)
    {

    }
}
