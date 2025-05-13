using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Enums;

namespace Nexus.Music.Choice.Domain.Services.EventArgs;

public class TrackQueueChangedEventArgs : System.EventArgs
{
    public Guid? Actor { get; set; }
    public string? TrackId { get; private set; }
    public int? Position { get; private set; }
    public TrackQueueEvent EventType { get; private set; }
    public IEnumerable<Track>? Items { get; private set; }

    public TrackQueueChangedEventArgs(string? trackId, TrackQueueEvent eventType, int? position = null)
    {
        TrackId = trackId;
        EventType = eventType;
        Position = position;
    }

    public TrackQueueChangedEventArgs(TrackQueueEvent eventType, IEnumerable<Track> queue)
    {
        EventType = eventType;
        Items = queue;
    }
}