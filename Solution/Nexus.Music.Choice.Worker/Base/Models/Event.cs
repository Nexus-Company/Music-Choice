using Nexus.Music.Choice.Worker.Base.Models.Enums;

namespace Nexus.Music.Choice.Worker.Base.Models;

public class Event
{
    public MessageType MessageType { get; set; }
    public EventType? EventType { get; set; }
    public object? Data { get; set; }
}

public enum MessageType
{
    Event,
    TrackQueue,
    PlayerState
}