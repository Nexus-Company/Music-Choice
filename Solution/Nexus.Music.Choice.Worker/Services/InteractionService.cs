namespace Nexus.Music.Choice.Worker.Services;

public interface IInteractionService
{
    Task TrackRemoveAsync(string trackId, Guid userId);
    Task TrackAddAync(string trackId, Guid userId);
}

internal class InteractionService : IInteractionService
{
    private readonly ILogger<InteractionService> _logger;
    private readonly InteractContext _interactContext;

    public InteractionService(InteractContext interactContext, ILogger<InteractionService> logger)
    {
        _logger = logger;
        _interactContext = interactContext;
    }

    public Task TrackAddAync(string trackId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task TrackRemoveAsync(string trackId, Guid userId)
    {
        throw new NotImplementedException();
    }
}