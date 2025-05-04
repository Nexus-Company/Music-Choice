using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Services;

public class VoteInteraction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public VotingType VotingType { get; set; }
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; }
}
