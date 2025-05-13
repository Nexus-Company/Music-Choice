using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Enums;

namespace Nexus.Music.Choice.Domain.Services.EventArgs;

public class PlayerStateChangedEventArgs : System.EventArgs
{
    public PlayerEvent EventType { get; set; }
    public PlayerState? OldState { get; }
    public PlayerState NewState { get; }

    public PlayerStateChangedEventArgs(PlayerEvent playerEvent, PlayerState? oldState, PlayerState newState)
    {
        EventType = playerEvent;
        OldState = oldState;
        NewState = newState;
    }
}