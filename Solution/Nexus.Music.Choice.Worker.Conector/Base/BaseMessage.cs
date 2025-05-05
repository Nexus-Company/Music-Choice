using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nexus.Music.Choice.Worker.Conector.Base;

public abstract class BaseMessage
{
    private protected static JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    internal abstract string GetJsonTextMessage();
}