using Nexus.Music.Choice.Worker.Base.Models.CommandData;
using Nexus.Music.Choice.Worker.Entities;
using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Services;

internal class UserConnectionControlService : IUserConnectionControlService
{
    private readonly Dictionary<int, HashSet<Guid>> _connectionUsers = [];
    private readonly Dictionary<Guid, int> _userConnections = [];
    private readonly InteractContext _interactionContext;
    private readonly ILogger<UserConnectionControlService> _logger;

    public event EventHandler<UserConnectionChangedEventArgs>? UsersConnectionChanged;

    public UserConnectionControlService(
        InteractContext interactionContext,
        ILogger<UserConnectionControlService> logger)
    {
        _interactionContext = interactionContext;
        _logger = logger;
    }

    public void ConnectUsers(int connectionId, IEnumerable<Guid> usersId)
    {
        if (!_connectionUsers.ContainsKey(connectionId))
            _connectionUsers[connectionId] = [];

        var newLogs = new List<UserConnectionTrace>();

        foreach (var userId in usersId)
        {
            if (_userConnections.ContainsKey(userId))
                continue;

            _connectionUsers[connectionId].Add(userId);
            _userConnections[userId] = connectionId;

            newLogs.Add(CreateTrace(userId, ConnectionState.Connect));
            _logger.LogInformation("User {UserId} connected on Connection {ConnectionId}.", userId, connectionId);
        }

        SaveTraces(newLogs);
        CallUserConnectionChangedEvent(usersId, ConnectionState.Connect, "System notify user connection.");
    }

    public void DisconnectUsers(int connectionId)
    {
        if (!_connectionUsers.TryGetValue(connectionId, out var usersId))
            return;

        var newLogs = new List<UserConnectionTrace>();

        foreach (var userId in usersId)
        {
            _userConnections.Remove(userId);
            newLogs.Add(CreateTrace(userId, ConnectionState.Disconnect));
            _logger.LogInformation("User {UserId} disconnected from Connection {ConnectionId}.", userId, connectionId);
        }

        _connectionUsers.Remove(connectionId);
        SaveTraces(newLogs);
        CallUserConnectionChangedEvent(usersId, ConnectionState.Disconnect, "System trace down.");
    }

    public void DisconnectUsers(IEnumerable<Guid> usersId)
    {
        var newLogs = new List<UserConnectionTrace>();

        foreach (var userId in usersId)
        {
            if (!_userConnections.TryGetValue(userId, out var connectionId))
                continue;

            _connectionUsers[connectionId].Remove(userId);
            _userConnections.Remove(userId);
            newLogs.Add(CreateTrace(userId, ConnectionState.Disconnect));
            _logger.LogInformation("User {UserId} disconnected from Connection {ConnectionId}.", userId, connectionId);

            if (_connectionUsers[connectionId].Count == 0)
                _connectionUsers.Remove(connectionId);
        }

        SaveTraces(newLogs);
        CallUserConnectionChangedEvent(usersId, ConnectionState.Disconnect, "System notify user disconnect.");
    }

    public int GetTotalActiveUsers() => _userConnections.Count;

    private static UserConnectionTrace CreateTrace(Guid userId, ConnectionState state) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            State = state,
            At = DateTime.UtcNow
        };

    private void SaveTraces(IEnumerable<UserConnectionTrace> traces)
    {
        if (!traces.Any())
            return;

        _interactionContext.UserConnectionsTrace.AddRange(traces);
        _interactionContext.SaveChanges();
    }

    private void CallUserConnectionChangedEvent(IEnumerable<Guid> usersId, ConnectionState connectionState, string? cause = null)
    {
        var args = new UserConnectionChangedEventArgs(usersId, connectionState)
        {
            OnlineUsers = GetTotalActiveUsers(),
            Cause = cause
        };

        UsersConnectionChanged?.Invoke(this, args);
    }
}