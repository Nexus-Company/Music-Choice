using Nexus.Music.Choice.Worker.Base.Models.Enums;

namespace Nexus.Music.Choice.Worker.Base.Models;

public class Event
{
    public Guid UserId { get; set; }
    public EventType EventType { get; set; }
    public object? Data { get; set; }
}