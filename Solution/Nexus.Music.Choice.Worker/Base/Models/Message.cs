using Newtonsoft.Json;
using Nexus.Music.Choice.Worker.Base.Models.Enums;

namespace Nexus.Music.Choice.Worker.Base.Models;

internal class Message
{
    public MessageType MessageType { get; set; }
    public object? Data { get => _messageData; }
    private readonly IMessageData? _messageData;

    public Message(MessageType messageType)
    {
        MessageType = messageType;
    }

    public Message(IMessageData messageData)
    {
        _messageData = messageData ?? throw new ArgumentNullException(nameof(messageData));
        MessageType = messageData.InternalType;
    }

    internal interface IMessageData
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public MessageType InternalType { get; }
    }
}