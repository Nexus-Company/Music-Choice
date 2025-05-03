using Nexus.Music.Choice.Domain;
using Nexus.Music.Choice.Worker.Base.Dispatcher;
using Nexus.Music.Choice.Worker.Base.Models;
using Nexus.Music.Choice.Worker.Base.Models.Enums;
using Nexus.Music.Choice.Worker.Interfaces;

namespace Nexus.Music.Choice.Worker.Services;

internal class InteractionService : IInteractionService
{
    private readonly ILogger<InteractionService> _logger;
    private readonly InteractContext _interactContext;
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IEventDispatcher<IStreamWriter> _eventDispatcher;

    public InteractionService(
        InteractContext interactContext,
        IEventDispatcher<IStreamWriter> eventDispatcher,
        IMusicPlayerService musicPlayerService,
        ILogger<InteractionService> logger)
    {
        _logger = logger;
        _interactContext = interactContext;
        _musicPlayerService = musicPlayerService;
    }

    public event EventHandler<Event> PlayerEvent;

    public Task TrackAddAync(string trackId, Guid userId, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public Task TrackDislikeAsync(string trackId, Guid userId, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public Task TrackLikeAsync(string trackId, Guid userId, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public Task TrackRemoveAsync(string trackId, Guid userId, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public Task VoteSkipAsync(Guid userId, CancellationToken? cancellationToken = null)
    {
        _eventDispatcher.DispatchEvent(new Event()
        {
            EventType = EventType.SkipMusic,
            UserId = userId,
            Data = null
        });
        throw new NotImplementedException();
    }
}