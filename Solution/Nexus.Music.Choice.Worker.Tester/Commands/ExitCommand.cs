namespace Nexus.Music.Choice.Worker.Tester.Commands;

class ExitCommand : ICommand
{
    private readonly Action _onQuit;

    public ExitCommand(Action onQuit)
    {
        _onQuit = onQuit;
    }

    public string Name => "quit";

    public Task<bool> ExecuteAsync(string argument)
    {
        _onQuit();
        return Task.FromResult(true);
    }
}
