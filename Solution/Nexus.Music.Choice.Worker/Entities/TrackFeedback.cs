namespace Nexus.Music.Choice.Worker.Entities;

public class TrackFeedback
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TrackId { get; set; }
    public FeedbackType Feedback { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum FeedbackType
{
    Like,
    Dislike
}
