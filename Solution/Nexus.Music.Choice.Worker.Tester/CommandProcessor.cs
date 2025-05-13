using Nexus.Music.Choice.Worker.Conector;
using Nexus.Music.Choice.Worker.Tester.Commands;

namespace Nexus.Music.Choice.Worker.Tester;

interface ICommand
{
    string Name { get; }
    Task<bool> ExecuteAsync(string argument);
}

public class CommandProcessor
{
    private readonly Dictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILocalUsersControl _localUsersControl;
    public CommandProcessor(PipeConector connector, Action onExit)
    {
        _localUsersControl = new LocalUsersControl();
        _commands["vote"] = new VoteCommand(connector);
        _commands["users"] = new UsersCommand(connector, _localUsersControl);
        _commands["quit"] = new ExitCommand(onExit);
        _commands["queue"] = new QueueCommand(connector, _localUsersControl);
    }

    public async Task<bool> ProcessAsync(string input)
    {
        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;

        var commandName = parts[0];
        var argument = parts.Length > 1 ? parts[1] : "";

        if (_commands.TryGetValue(commandName, out var command))
        {
            return await command.ExecuteAsync(argument);
        }
        else
        {
            Console.WriteLine("Comando desconhecido.");
        }

        return false;
    }
}