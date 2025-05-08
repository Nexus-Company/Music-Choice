using Nexus.Music.Choice.Worker.Conector;

namespace Nexus.Music.Choice.Worker.Tester.Commands;

class VoteCommand : ICommand
{
    private readonly PipeConector _connector;

    public VoteCommand(PipeConector connector)
    {
        _connector = connector;
    }

    public string Name => "send";

    public async Task<bool> ExecuteAsync(string argument)
    {
        throw new NotImplementedException();
    }
}
