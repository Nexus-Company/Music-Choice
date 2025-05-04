using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Services;

internal class FeedBackService : IFeedbackService
{
    public Task TrackDislikeAsync(Guid userId, string trackId, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public Task TrackLikeAsync(Guid userId, string trackId, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
}