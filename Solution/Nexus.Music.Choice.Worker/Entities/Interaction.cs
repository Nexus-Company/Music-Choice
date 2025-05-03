using Nexus.Music.Choice.Worker.Entities.Enums;

namespace Nexus.Music.Choice.Worker.Entities;

public class Interaction
{
    public InteractionType Type { get; set; }
    public DateTime Date { get; set; }
    public Guid UserId { get; set; }
}