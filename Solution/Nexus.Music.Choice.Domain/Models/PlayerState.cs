namespace Nexus.Music.Choice.Domain.Models;

public abstract class PlayerState
{
    public virtual bool IsPlaying { get; set; }
    public virtual long TimeStamp { get; set; }
    public virtual int ProgressMilisseconds { get; set; }
    public virtual Track? Item { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not PlayerState other)
            return false;

        return Equals(Item, other.Item)
            && IsPlaying == other.IsPlaying;
    }

    public override int GetHashCode()
    {
        // Usa apenas os campos relevantes
        return HashCode.Combine(Item, IsPlaying);
    }
}