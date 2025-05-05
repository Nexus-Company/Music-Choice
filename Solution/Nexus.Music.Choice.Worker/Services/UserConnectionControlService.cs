using Nexus.Music.Choice.Worker.Base.Models.Data;
using Nexus.Music.Choice.Worker.Entities;
using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker.Services;

internal class UserConnectionControlService : IUserConnectionControlService
{
    private readonly Dictionary<int, HashSet<Guid>> _connectionUsers = [];
    private readonly Dictionary<Guid, int> _userConnections = [];
    private readonly InteractContext _interactionContext;
    private readonly ILogger<UserConnectionControlService> _logger;

    public UserConnectionControlService(
        InteractContext interactionContext,
        ILogger<UserConnectionControlService> logger)
    {
        _interactionContext = interactionContext;
        _logger = logger;
    }

    public void ConnectUsers(int connectionId, IEnumerable<Guid> userIds)
    {
        if (!_connectionUsers.ContainsKey(connectionId))
            _connectionUsers[connectionId] = [];

        var newLogs = new List<UserConnectionTrace>();

        foreach (var userId in userIds)
        {
            if (_userConnections.ContainsKey(userId))
                continue;

            _connectionUsers[connectionId].Add(userId);
            _userConnections[userId] = connectionId;

            newLogs.Add(CreateTrace(userId, ConnectionState.Connect));
            _logger.LogInformation("User {UserId} connected on Connection {ConnectionId}.", userId, connectionId);
        }

        SaveTraces(newLogs);
    }

    public void DisconnectUsers(int connectionId)
    {
        if (!_connectionUsers.TryGetValue(connectionId, out var users))
            return;

        var newLogs = new List<UserConnectionTrace>();

        foreach (var userId in users)
        {
            _userConnections.Remove(userId);
            newLogs.Add(CreateTrace(userId, ConnectionState.Disconnect));
            _logger.LogInformation("User {UserId} disconnected from Connection {ConnectionId}.", userId, connectionId);
        }

        _connectionUsers.Remove(connectionId);
        SaveTraces(newLogs);
    }

    public void DisconnectUsers(IEnumerable<Guid> userIds)
    {
        var newLogs = new List<UserConnectionTrace>();

        foreach (var userId in userIds)
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
    }

    public int GetTotalActiveUsers() => _userConnections.Count;

    private UserConnectionTrace CreateTrace(Guid userId, ConnectionState state) =>
        new UserConnectionTrace
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
}
