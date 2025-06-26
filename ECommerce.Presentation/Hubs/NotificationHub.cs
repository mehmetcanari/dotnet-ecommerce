using ECommerce.Application.Abstract.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Presentation.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILoggingService _logger;
    private readonly ICurrentUserService _currentUserService;
    private static readonly Dictionary<string, string> _userConnections = new();

    public NotificationHub(ILoggingService logger, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public override async Task OnConnectedAsync()
    {
        var result = _currentUserService.GetCurrentUserId();
        if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
        {
            _userConnections[result.Data] = Context.ConnectionId;
            _logger.LogInformation("User {UserId} connected to notification hub with connection {ConnectionId}", 
                result.Data, Context.ConnectionId);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var result = _currentUserService.GetCurrentUserId();
        if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
        {
            _userConnections.Remove(result.Data);
            _logger.LogInformation("User {UserId} disconnected from notification hub", result.Data);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
} 