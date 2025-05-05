﻿namespace Nexus.Music.Choice.Domain.Models;

public abstract class PlayerState
{
    public virtual bool IsPlaying { get; set; }
    public virtual long TimeStamp { get; set; }
    public long RetrievedAt { get; set; }
    public virtual int ProgressMilisseconds { get; set; }
    public virtual int? Volume { get; set; }
    public virtual string? Repeat { get; set; }
    public virtual Track? Item { get; set; }

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