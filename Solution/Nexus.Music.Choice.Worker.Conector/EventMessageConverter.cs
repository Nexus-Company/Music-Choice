using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Music.Choice.Worker.Conector.Base;
using Nexus.Music.Choice.Worker.Conector.EventData;

namespace Nexus.Music.Choice.Worker.Conector;

internal class EventMessageConverter : JsonConverter
{
    private readonly Dictionary<MessageType, Type> _resolveType;
    public EventMessageConverter()
    {
        _resolveType = [];

        _resolveType.Add(MessageType.TrackQueueChanged, typeof(TrackQueueChangedEventData));
        _resolveType.Add(MessageType.InitialMessage, typeof(InitialEventData));
        _resolveType.Add(MessageType.PlayerStateChanged, typeof(PlayerStateChangedEventData));
        _resolveType.Add(MessageType.UserEvent, typeof(UserConnectionEventData));
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(EventMessage).IsAssignableFrom(objectType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var messageTypeValue = jObject["MessageType"]?.ToString();

        if (!Enum.TryParse(messageTypeValue, out MessageType messageType))
            throw new JsonSerializationException("Tipo de mensagem inválido");

        var dataToken = jObject["Data"];
        var dataType = _resolveType[messageType];

        var data = dataType != null && dataToken != null
            ? dataToken.ToObject(dataType, serializer)
            : null;

        return new EventMessage
        {
            MessageType = messageType,
            Data = (IEventData?)data
        };
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var eventMessage = (EventMessage)value!;
        var jo = new JObject
        {
            ["MessageType"] = eventMessage.MessageType.ToString(),
            ["Data"] = eventMessage.Data != null ? JToken.FromObject(eventMessage.Data, serializer) : null
        };
        jo.WriteTo(writer);
    }
}
