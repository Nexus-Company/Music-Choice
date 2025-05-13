using Nexus.Music.Choice.Worker.Conector;
using Nexus.Music.Choice.Worker.Conector.Messages;

namespace Nexus.Music.Choice.Worker.Tester.Commands;

internal class QueueCommand : ICommand
{
    private readonly PipeConector _pipeConector;
    private readonly ILocalUsersControl _localUsersControl;
    public string Name => "queue";

    public QueueCommand(PipeConector pipeConector, ILocalUsersControl localUsersControl)
    {
        _pipeConector = pipeConector;
        _localUsersControl = localUsersControl;
    }

    public Task<bool> ExecuteAsync(string argument)
    {
        var parts = argument.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
            return Task.FromResult(false);

        if (Enum.TryParse(parts[0], true, out QueueAction queueAction))
        {
            string? trackId = parts[1];
            Guid? userId = _localUsersControl.RandomUserId();

            if (string.IsNullOrWhiteSpace(trackId))
            {
                Console.WriteLine("Track ID cannot be null or empty.");
                return Task.FromResult(false);
            }

            if (!userId.HasValue)
            {
                Console.WriteLine("One user connected is required for this action.");
                return Task.FromResult(false);
            }

            switch (queueAction)
            {
                case QueueAction.Add:
                    _pipeConector.SendMessage(new QueueChangeMessage(userId.Value, trackId));
                    break;

                case QueueAction.Remove:
                    throw new NotImplementedException();

                default:
                    break;
            }
        }
        else
        {
            Console.WriteLine("Use: queue <action> <trackId>");
        }

        return Task.FromResult(true);
    }
}

public enum QueueAction
{
    Add,
    Remove
}