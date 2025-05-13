namespace Nexus.Music.Choice.Worker.Base.Models.CommandData;

public class QueueChangeCommandData : ICommandData
{
    public Guid UserId { get; set; }
    public string TrackId { get; set; }
    public int? Position { get; set; }
}