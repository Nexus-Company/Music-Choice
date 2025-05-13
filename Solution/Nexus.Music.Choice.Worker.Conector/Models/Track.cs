namespace Nexus.Music.Choice.Worker.Conector.Models;

public class Track
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Duration { get; set; }
    public bool Explit { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Track other)
            return false;

        return other.Id == Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Duration, Explit);
    }
}