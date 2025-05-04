namespace Nexus.Music.Choice.Worker.Entities;

public class ActionExecuted
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public ActionExecutedType ActionExecutedType { get; set; }
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum ActionExecutedType
{
    SkipTrack,
    TrackQueueRemove,
    TrackQueueRemoveByVote,
    TrackQueueAdd,
    SkipTrackByVote
}
