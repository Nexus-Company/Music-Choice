using Nexus.Music.Choice.Worker.Base.Models.Enums;

namespace Nexus.Music.Choice.Worker.Base.Models;

public class Command
{
    public ActionType ActionType { get; set; }
    public object? Data { get; set; }
}

public interface ICommandData
{

}