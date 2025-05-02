namespace Nexus.Music.Choice.Worker.Models;

public class InteractionEvent
{
    public ActionType Action { get; set; }
    public Guid ActorId { get; set; }
    public string? SongId { get; set; }
}

public enum ActionType
{
    TrackAdd,
    TrackRemove
}