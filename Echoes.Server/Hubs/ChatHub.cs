using Echoes.Server.ConnectionManager;
using Microsoft.AspNetCore.SignalR;

namespace Echoes.Server.Hubs;

public class ChatHub(UserConnectionManager connectionManager) : Hub
{
    private readonly UserConnectionManager _connectionManager = connectionManager;

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var userId = httpContext?.Request.Query["userId"].ToString();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            _connectionManager.AddUser(userId, Context.ConnectionId);
            await BroadcastOnlineUsers();
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectionManager.RemoveByConnectionId(Context.ConnectionId);
        await BroadcastOnlineUsers();
        await base.OnDisconnectedAsync(exception);
    }

    private async Task BroadcastOnlineUsers()
    {
        var onlineUserIds = _connectionManager.GetOnlineUserIds();
        await Clients.All.SendAsync("GetOnlineUsers", onlineUserIds);
    }
}
