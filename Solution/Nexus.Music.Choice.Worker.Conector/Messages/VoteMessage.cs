using Nexus.Music.Choice.Worker.Conector.Base;
using Nexus.Music.Choice.Worker.Conector.Enums;
using System.Text.Json;

namespace Nexus.Music.Choice.Worker.Conector.Messages;

public class VoteMessage : BaseMessage
{
    public VotingType Type { get; set; }
    public Guid UserId { get; set; }
    public string? TrackId { get; set; }

    public VoteMessage(VotingType type, Guid userId, string? trackId = null)
    {
        Type = type;
        UserId = userId;

        if (type == VotingType.TrackQueueRemove && string.IsNullOrWhiteSpace(trackId))
            throw new ArgumentException("TrackId cannot be null or empty when voting for TrackQueueRemove.");

        TrackId = trackId;
    }

    internal override string GetJsonTextMessage()
    {
        return JsonSerializer.Serialize(new
        {
            ActionType = "Vote",
            UserId,
            Data = new
            {
                Type,
                TrackId
            }
        }, JsonSerializerOptions);
    }
}