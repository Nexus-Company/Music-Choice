namespace Nexus.Music.Choice.Worker.Interfaces;

public interface IInteractionService
{
    Task TrackRemoveAsync(string trackId, Guid userId, CancellationToken? cancellationToken = default);
    Task TrackAddAync(string trackId, Guid userId, CancellationToken? cancellationToken = default);
    Task TrackLikeAsync(string trackId, Guid userId, CancellationToken? cancellationToken = default);
    Task TrackDislikeAsync(string trackId, Guid userId, CancellationToken? cancellationToken = default);
    Task VoteSkipAsync(Guid userId, CancellationToken? cancellationToken = default);
}