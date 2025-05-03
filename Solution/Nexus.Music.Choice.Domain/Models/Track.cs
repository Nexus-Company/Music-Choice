namespace Nexus.Music.Choice.Domain.Models;

public abstract class Track
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Duration { get; set; }
}
