namespace Nexus.Music.Choice.Domain.Models;

public abstract class PlayerState
{
    public bool IsPlaying { get; set; }
    public long TimeStamp { get; set; }
    public int ProgressMilisseconds { get; set; }
    public Track Item { get; set; }
}