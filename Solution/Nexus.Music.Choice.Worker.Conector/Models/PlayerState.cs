namespace Nexus.Music.Choice.Worker.Conector.Models;

public class PlayerState
{
    public bool IsPlaying { get; set; }
    public long TimeStamp { get; set; }
    public long RetrievedAt { get; set; }
    public int ProgressMilisseconds { get; set; }
    public int? Volume { get; set; }
    public string? Repeat { get; set; }
    public Track? Item { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not PlayerState other)
            return false;

        return Equals(Item, other.Item)
            && IsPlaying == other.IsPlaying && Volume == other.Volume;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Item, IsPlaying, Volume);
    }
}