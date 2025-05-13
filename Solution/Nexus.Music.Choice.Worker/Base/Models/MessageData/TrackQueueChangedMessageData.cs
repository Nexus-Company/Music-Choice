using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.Enums;
using Nexus.Music.Choice.Domain.Services.EventArgs;
using Nexus.Music.Choice.Worker.Base.Models.Enums;

namespace Nexus.Music.Choice.Worker.Base.Models.MessageData;

internal class TrackQueueChangedMessageData : Message.IMessageData
{
    public Guid? Actor { get; private set; }
    public string? TrackId { get; private set; }
    public TrackQueueEvent EventType { get; private set; }
    public IEnumerable<Track>? Queue { get; private set; }
    public MessageType InternalType => MessageType.TrackQueueChanged;

    public TrackQueueChangedMessageData(
        TrackQueueEvent eventType,
        Guid? actor = null,
        string? trackId = null,
        IEnumerable<Track>? queue = null)
    {
        EventType = eventType;
        Actor = actor;
        TrackId = trackId;
        Queue = queue;

        if (TrackId == null || Queue == null)
            throw new ArgumentNullException("TrackId and Queue cannot be null.");
    }

    public TrackQueueChangedMessageData(TrackQueueChangedEventArgs args)
        : this(args.EventType, args.Actor, args.TrackId)
    {

    }
}