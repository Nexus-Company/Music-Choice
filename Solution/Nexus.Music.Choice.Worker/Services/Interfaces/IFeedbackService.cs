namespace Nexus.Music.Choice.Worker.Services.Interfaces;

public interface IFeedbackService
{
    Task TrackLikeAsync(Guid userId, string trackId, CancellationToken? cancellationToken = default);
    Task TrackDislikeAsync(Guid userId, string trackId, CancellationToken? cancellationToken = default);
}