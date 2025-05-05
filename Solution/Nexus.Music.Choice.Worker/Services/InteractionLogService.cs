using Nexus.Music.Choice.Domain;
using Nexus.Music.Choice.Worker.Entities;
using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Services;

public class InteractionLogService : IInteractionLogService
{
    private readonly InteractContext _context;
    private readonly ILogger<InteractionLogService> _logger;
    private readonly IClock _clock;

    public InteractionLogService(InteractContext context, IClock clock, ILogger<InteractionLogService> logger)
    {
        _context = context;
        _logger = logger;
        _clock = clock;
    }

    public async Task LogFeedbackAsync(Guid userId, string trackId, FeedbackType feedback, CancellationToken? cancellationToken = default)
    {
        var feedbackEntry = new TrackFeedback
        {
            UserId = userId,
            TrackId = trackId,
            Feedback = feedback,
            Timestamp = _clock.Now
        };

        _context.TrackFeedbacks.Add(feedbackEntry);
        await _context.SaveChangesAsync(cancellationToken ?? CancellationToken.None);

        _logger.LogInformation("Feedback '{Feedback}' logged for track {TrackId} by user {UserId}.", feedback, trackId, userId);
    }

    public async Task LogVoteAsync(Guid userId, VotingType votingType, object? data, CancellationToken? cancellationToken = default)
    {
        var voteLog = new VoteInteraction
        {
            UserId = userId,
            VotingType = votingType,
            Data = data,
            Timestamp = _clock.Now
        };

        _context.VoteInteractions.Add(voteLog);
        await _context.SaveChangesAsync(cancellationToken ?? CancellationToken.None);

        _logger.LogInformation("Vote for '{VotingType}' logged for track '{Data}' by user '{UserId}'.", votingType, data, userId);
    }

    public async Task LogActionExecutedAsync(ActionExecutedType actionExecutedType, object? data, Guid? userId = null, CancellationToken? cancellationToken = default)
    {
        var actionLog = new ActionExecuted
        {
            UserId = userId,
            ActionExecutedType = actionExecutedType,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        _context.ActionsExecuted.Add(actionLog);
        await _context.SaveChangesAsync(cancellationToken ?? CancellationToken.None);

        _logger.LogInformation("Action '{ActionExecuted}' executed for track {Data} by user {UserId}.", actionExecutedType, data, userId?.ToString() ?? "system");
    }
}