using Nexus.Music.Choice.Worker.Conector.Base;
using Nexus.Music.Choice.Worker.Conector.Models;

namespace Nexus.Music.Choice.Worker.Conector.EventData;

public class TrackQueueChangedEventData : IEventData
{
    public Guid? Actor { get; set; }
    public string? TrackId { get; set; }
    public TrackQueueEvent EventType { get; set; }
    public IEnumerable<Track>? Queue { get; set; }
}

public enum TrackQueueEvent
{
    Added,
    Removed,
    Reordered
}