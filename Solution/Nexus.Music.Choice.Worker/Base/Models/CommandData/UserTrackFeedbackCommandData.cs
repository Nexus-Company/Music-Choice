using Nexus.Music.Choice.Worker.Entities;

namespace Nexus.Music.Choice.Worker.Base.Models.CommandData;

public class UserTrackFeedbackCommandData : ICommandData
{
    public Guid UserId { get; set; }
    public string TrackId { get; set; }
    public FeedbackType Type { get; set; }
}