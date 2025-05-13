using Nexus.Music.Choice.Worker.Conector.Base;
using Nexus.Music.Choice.Worker.Conector.Models;

namespace Nexus.Music.Choice.Worker.Conector.EventData;

public class InitialEventData : IEventData
{
    public PlayerState? PlayerState { get; set; }
    public IEnumerable<Track>? Queue { get; set; }
    public int OnlineUsers { get; set; }
}