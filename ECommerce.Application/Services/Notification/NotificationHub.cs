using ECommerce.Application.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Application.Services.Notification;

[Authorize]
public class NotificationHub(ICurrentUserService currentUserService) : Hub
{
    private static readonly Dictionary<string, string> _connections = [];

    public static bool IsUserConnected(string userId) => _connections.ContainsKey(userId);

    public static string? GetUserConnectionId(string userId) => _connections.TryGetValue(userId, out var connectionId) ? connectionId : null;

    public static int GetTotalConnections() => _connections.Count;

    public override async Task OnConnectedAsync()
    {
        var userId = currentUserService.GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            _connections[userId] = Context.ConnectionId;
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = currentUserService.GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            _connections.Remove(userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
} 