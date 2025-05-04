namespace Nexus.Music.Choice.Worker.Services.Interfaces;

public interface IInteractionService
{
    Task TrackRemoveAsync(string trackId, Guid userId, CancellationToken? cancellationToken = default);
    Task TrackAddAync(string trackId, Guid userId, CancellationToken? cancellationToken = default);
    Task VoteSkipAsync(Guid userId, CancellationToken? cancellationToken = default);
}

public enum VotingType
{
    SkipTrack,
    TrackQueueRemove
}