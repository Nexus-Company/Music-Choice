using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Base.Models.Data;

public class VotingCommandData : ICommandData
{
    public Guid UserId { get; set; }
    public string? TrackId { get; set; }
    public VotingType Type { get; set; }
}