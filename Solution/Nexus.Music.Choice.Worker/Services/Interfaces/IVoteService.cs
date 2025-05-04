namespace Nexus.Music.Choice.Worker.Services.Interfaces;

public interface IVoteService
{
    public VotingConfig VotingConfig { get; set; }
    Task<bool> ShouldActionBePerformedAsync(VotingType votingType, object? data = null, CancellationToken? cancellationToken = default);
    void ResetVotesForAction(VotingType votingType, object? data = null);
    void AddVote(Guid userId, VotingType votingType, object? data = null);
}

public enum VotingStrategy
{
    Majority,        // Ação é feita se mais da metade votou
    Unanimous,       // Ação é feita apenas se todos votaram
    FixedThreshold,  // Ação é feita se um número fixo de votos for atingido
    Percentage       // Ação é feita se certa porcentagem de votos for atingida
}

public class VotingConfig
{
    public VotingStrategy Strategy { get; set; }
    public int Threshold { get; set; }
    public double Percentage { get; set; }
}