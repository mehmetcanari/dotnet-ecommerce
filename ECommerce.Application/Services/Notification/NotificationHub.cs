using ECommerce.Application.Abstract.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Application.Services.Notification;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ICurrentUserService _currentUserService;
    private static readonly Dictionary<string, string> _userConnections = [];

    public NotificationHub(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public static IReadOnlyDictionary<string, string> UserConnections => _userConnections;

    public static bool IsUserConnected(string userId) => _userConnections.ContainsKey(userId);

    public static string? GetUserConnectionId(string userId) => _userConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;

    public static int GetTotalConnections() => _userConnections.Count;

    public async Task JoinUserGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _currentUserService.GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections[userId] = Context.ConnectionId;
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUserService.GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections.Remove(userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
} 