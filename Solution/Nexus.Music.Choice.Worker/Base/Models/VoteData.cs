using Nexus.Music.Choice.Worker.Base.Models.Enums;
using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Base.Models;

public class VoteData
{
    public string? TrackId { get; set; }
    public VotingType Type { get; set; }
}