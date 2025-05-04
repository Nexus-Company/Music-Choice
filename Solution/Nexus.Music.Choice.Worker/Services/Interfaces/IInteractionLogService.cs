using Nexus.Music.Choice.Worker.Entities;

namespace Nexus.Music.Choice.Worker.Services.Interfaces;

public interface IInteractionLogService
{
    Task LogFeedbackAsync(Guid userId, string trackId, FeedbackType feedback, CancellationToken? cancellationToken = default);

    // Loga a ação de voto
    Task LogVoteAsync(Guid userId, VotingType votingType, object? data, CancellationToken? cancellationToken = default);

    // Loga a execução de uma ação, ou seja, a ação realizada após a votação
    public Task LogActionExecutedAsync(ActionExecutedType actionExecutedType, object? data, Guid? userId = null, CancellationToken? cancellationToken = default);
}