using Nexus.Music.Choice.Worker.Base.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nexus.Music.Choice.Worker;

internal static class CommandUtils
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static T GetCommandData<T>(this Command command)
            where T : class, ICommandData
    {
        var excp = new ArgumentException("The type of data in message is incorrect.");

        if (command.Data == null || command.Data is not JsonElement jsonElement)
            throw excp;

        return jsonElement.Deserialize<T>(jsonOptions) ?? throw excp;
    }
}