namespace Nexus.Music.Choice.Domain.Models;

public abstract class Track
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Duration { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Track other)
            return false;

        return other.Id == Id;
    }

    public override int GetHashCode()
    {
        // Usa apenas os campos relevantes
        return HashCode.Combine(Id, Name, Duration);
    }
}