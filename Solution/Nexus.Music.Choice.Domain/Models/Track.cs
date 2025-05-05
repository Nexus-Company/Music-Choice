namespace Nexus.Music.Choice.Domain.Models;

public abstract class Track
{
    public virtual string Id { get; set; }
    public virtual string Name { get; set; }
    public virtual int Duration { get; set; }
    public virtual bool Explit { get; set; }

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