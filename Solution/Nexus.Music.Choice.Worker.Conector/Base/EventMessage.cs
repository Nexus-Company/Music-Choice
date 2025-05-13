namespace Nexus.Music.Choice.Worker.Conector.Base;

public class EventMessage
{
    public MessageType MessageType { get; set; }
    public IEventData? Data { get; set; }
}

public interface IEventData
{

}

public enum MessageType
{
    UserConnectionEvent,
    TrackQueueChanged,
    PlayerStateChanged,
    InitialMessage
}