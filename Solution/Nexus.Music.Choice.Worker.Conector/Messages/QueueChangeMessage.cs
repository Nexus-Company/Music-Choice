using Nexus.Music.Choice.Worker.Conector.Base;
using System.Text.Json;

namespace Nexus.Music.Choice.Worker.Conector.Messages;

public class QueueChangeMessage : BaseMessage
{
    public Guid UserId { get; set; }
    public string TrackId { get; set; }
    public int? Position { get; set; }

    public QueueChangeMessage(Guid userId, string trackId, int? position = null)
    {
        UserId = userId;
        TrackId = trackId;
        Position = position;
    }

    internal override string GetJsonTextMessage()
    {
        return JsonSerializer.Serialize(new
        {
            ActionType = "QueueChange",
            Data = new
            {
                UserId,
                TrackId,
                Position
            }
        }, JsonSerializerOptions);
    }
}