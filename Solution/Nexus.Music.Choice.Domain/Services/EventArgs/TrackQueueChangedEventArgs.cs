using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Enums;

namespace Nexus.Music.Choice.Domain.Services.EventArgs;

public class TrackQueueChangedEventArgs : System.EventArgs
{
    public Guid? Actor { get; private set; }
    public string? TrackId { get; private set; }
    public TrackQueueEvent EventType { get; private set; }
    public IEnumerable<Track>? Queue { get; private set; }
}