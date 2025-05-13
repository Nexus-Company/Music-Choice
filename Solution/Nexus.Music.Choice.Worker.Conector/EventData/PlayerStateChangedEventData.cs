using Nexus.Music.Choice.Worker.Conector.Base;
using Nexus.Music.Choice.Worker.Conector.Models;

namespace Nexus.Music.Choice.Worker.Conector.EventData;

public class PlayerStateChangedEventData : IEventData
{
    public PlayerEvent PlayerEvent { get; set; }
    public PlayerState? NewPlayerState { get; set; }
    public PlayerState? OldPlayerState { get; set; }
}

public enum PlayerEvent
{
    Play,
    Pause,
    Stop,
    Next,
    Previous,
    Seek,
    VolumeChange,
    Mute,
    Unmute
}