using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Base.Models.CommandData;

public class VotingCommandData : ICommandData
{
    public Guid UserId { get; set; }
    public string? TrackId { get; set; }
    public VotingType Type { get; set; }
}