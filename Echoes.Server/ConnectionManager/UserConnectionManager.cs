using System.Collections.Concurrent;

namespace Echoes.Server.ConnectionManager;

/// <summary>
/// Thread-safe in-memory registry of authenticated userId -> SignalR connectionId.
/// Mirrors the Node.js userSocketMap used by Socket.io.
/// </summary>
public class UserConnectionManager
{
    private readonly ConcurrentDictionary<string, string> _userIdToConnectionId = new();
    private readonly ConcurrentDictionary<string, string> _connectionIdToUserId = new();

    public void AddUser(string userId, string connectionId)
    {
        _userIdToConnectionId[userId] = connectionId;
        _connectionIdToUserId[connectionId] = userId;
    }

    public void RemoveByConnectionId(string connectionId)
    {
        if (_connectionIdToUserId.TryRemove(connectionId, out var userId))
        {
            _userIdToConnectionId.TryRemove(userId, out _);
        }
    }

    public void RemoveByUserId(string userId)
    {
        if (_userIdToConnectionId.TryRemove(userId, out var connectionId))
        {
            _connectionIdToUserId.TryRemove(connectionId, out _);
        }
    }

    public string? GetConnectionId(string userId)
    {
        return _userIdToConnectionId.TryGetValue(userId, out var connectionId) ? connectionId : null;
    }

    public IReadOnlyCollection<string> GetOnlineUserIds()
    {
        return _userIdToConnectionId.Keys.ToList();
    }
}
