using Nexus.Music.Choice.Worker.Conector.Base;

namespace Nexus.Music.Choice.Worker.Conector.Messages;

public class UserTrackFeedBackMessage : BaseMessage
{
    public Guid UserId { get; set; }
    public string TrackId { get; set; }
    public FeedBackType Type { get; set; }

    internal override string GetJsonTextMessage()
    {
        throw new NotImplementedException();
    }
}

public enum FeedBackType
{
    Like,
    Dislike
}