using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Enums;
using Nexus.Music.Choice.Domain.Services.EventArgs;
using Nexus.Music.Choice.Worker.Base.Models.Enums;

namespace Nexus.Music.Choice.Worker.Base.Models.MessageData;

internal class TrackQueueChangedMessageData : Message.IMessageData
{
    public MessageType InternalType => MessageType.TrackQueueChanged;

    public Guid? Actor { get; set; }
    public string? TrackId { get; private set; }
    public int? Position { get; private set; }
    public TrackQueueEvent EventType { get; private set; }
    public IEnumerable<Track>? Queue { get; private set; }

    public TrackQueueChangedMessageData(
        TrackQueueEvent eventType,
        Guid? actor = null,
        string? trackId = null,
        int? position = null,
        IEnumerable<Track>? queue = null)
    {
        EventType = eventType;
        Actor = actor;
        TrackId = trackId;
        Queue = queue;
        Position = position;

        if (TrackId == null && Queue == null)
            throw new ArgumentNullException("TrackId and Queue cannot be null.");
    }

    public TrackQueueChangedMessageData(TrackQueueChangedEventArgs args)
        : this(args.EventType, args.Actor, args.TrackId, args.Position, args.Queue)
    {

    }
}