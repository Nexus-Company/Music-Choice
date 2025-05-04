using Nexus.Music.Choice.Domain.Services;
using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Services;

internal class VoteService : IVoteService, IDisposable
{
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly Dictionary<KeyVotingType, HashSet<Guid>> _votingsCount = [];
    public VotingConfig VotingConfig { get; set; }

    public VoteService(IMusicPlayerService musicPlayerService, VotingConfig? votingConfig = null)
    {
        _musicPlayerService = musicPlayerService;
        _musicPlayerService.PlayerStateChanged += PlayerStateChanged;

        foreach (VotingType votingType in Enum.GetValues<VotingType>())
        {
            if (votingType == VotingType.TrackQueueRemove)
                continue;

            _votingsCount.Add(new KeyVotingType(votingType), []);
        }

        VotingConfig = votingConfig ?? new VotingConfig
        {
            Strategy = VotingStrategy.FixedThreshold,
            Threshold = 3,
            Percentage = 0.75
        };
    }

    public void AddVote(Guid userId, VotingType votingType, object? data = null)
    {
        if (_votingsCount.TryGetValue(new KeyVotingType(votingType, data), out HashSet<Guid>? votes))
        {
            if (votes.Contains(userId))
                return;

            votes.Add(userId);
        }
        else
        {
            throw new ArgumentException($"O tipo de votação {votingType} não é suportado.");
        }
    }

    public void ResetVotesForAction(VotingType votingType, object? data = null)
    {
        if (_votingsCount.TryGetValue(new KeyVotingType(votingType, data), out HashSet<Guid>? votes))
            votes.Clear();
    }

    public Task<bool> ShouldActionBePerformedAsync(VotingType votingType, object? data = null, CancellationToken? cancellationToken = null)
    {
        if (!_votingsCount.TryGetValue(new KeyVotingType(votingType, data), out HashSet<Guid>? votes))
            throw new ArgumentException($"O tipo de votação {votingType} não é suportado.");

        int voteCount = votes.Count;

        switch (VotingConfig.Strategy)
        {
            case VotingStrategy.Majority:
                int totalUsers = GetTotalActiveUsers();
                return Task.FromResult(voteCount > (totalUsers / 2));

            case VotingStrategy.Unanimous:
                totalUsers = GetTotalActiveUsers();
                return Task.FromResult(voteCount == totalUsers);

            case VotingStrategy.FixedThreshold:
                return Task.FromResult(voteCount >= VotingConfig.Threshold);

            case VotingStrategy.Percentage:
                double percentage = GetVotePercentage(votingType);
                return Task.FromResult(percentage >= VotingConfig.Percentage); // idem

            default:
                throw new InvalidOperationException($"Estratégia de votação não reconhecida: {VotingConfig.Strategy}");
        }
    }

    private void PlayerStateChanged(object? sender, PlayerStateChangedEventArgs e)
    {
        ResetVotesForAction(VotingType.SkipTrack);
    }

    private double GetVotePercentage(VotingType votingType)
    {
        int totalUsers = GetTotalActiveUsers();

        if (totalUsers == 0)
            return 0.0;

        if (!_votingsCount.TryGetValue(new KeyVotingType(votingType), out HashSet<Guid>? votes))
            return 0.0;

        return votes.Count / (double)totalUsers * 100.0;
    }

    private int GetTotalActiveUsers()
    {
        return 1; // Exemplo fixo, substituir pela lógica real
    }

    public void Dispose()
    {
        _musicPlayerService.PlayerStateChanged -= PlayerStateChanged;
        _votingsCount.Clear();
    }
}

public struct KeyVotingType
{
    public VotingType Type { get; set; }
    public object? Data { get; set; }

    public KeyVotingType(VotingType type,
                         object? data = null)
    {
        Type = type;
        Data = data;
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is KeyVotingType other)
            return other == this;

        return false;
    }

    public override readonly int GetHashCode()
    {
        if (Data == null)
            return Type.GetHashCode();

        return HashCode.Combine(Type, Data.GetHashCode());
    }

    public static bool operator ==(KeyVotingType left, KeyVotingType right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(KeyVotingType left, KeyVotingType right)
    {
        return !(left == right);
    }
}