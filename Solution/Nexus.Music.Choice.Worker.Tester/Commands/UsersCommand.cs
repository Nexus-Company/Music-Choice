using Newtonsoft.Json;
using Nexus.Music.Choice.Worker.Conector;
using Nexus.Music.Choice.Worker.Conector.Enums;
using Nexus.Music.Choice.Worker.Conector.Messages;

namespace Nexus.Music.Choice.Worker.Tester.Commands;

class UsersCommand : ICommand
{
    PipeConector _pipeConector;

    public UsersCommand(PipeConector pipeConector)
    {
        _pipeConector = pipeConector;
    }

    public string Name => "users";

    public Task<bool> ExecuteAsync(string argument)
    {
        var parts = argument.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
            return Task.FromResult(false);

        if (
            Enum.TryParse(parts[0], true, out ConnectionNotifyType connectionType))
        {
            Guid[]? ids = null;

            try
            {
                ids = JsonConvert.DeserializeObject<Guid[]>(parts[1]);
            }
            catch (Exception)
            {
            }

            if (ids != null)
            {
                _pipeConector.SendMessage(new ConnectionNotifyMessage(ids, connectionType));

                Console.WriteLine($"User '{parts[1]}' connection state '{connectionType}' changed.");
            }
            else if (Guid.TryParse(parts[1], out var userId))
            {
                _pipeConector.SendMessage(new ConnectionNotifyMessage([userId], connectionType));

                Console.WriteLine($"User '{userId}' connection state '{connectionType}' changed.");
            }
            else
                return Task.FromResult(false);
        }
        else
        {
            Console.WriteLine("Use: users <state> <guid>");
        }

        return Task.FromResult(true);
    }
}
