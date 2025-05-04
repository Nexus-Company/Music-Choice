namespace Nexus.Music.Choice.Domain.Models;

public abstract class PlayerState
{
    public virtual bool IsPlaying { get;  }
    public virtual long TimeStamp { get; }
    public virtual int ProgressMilisseconds { get; }
    public virtual Track? Item { get;  }
}