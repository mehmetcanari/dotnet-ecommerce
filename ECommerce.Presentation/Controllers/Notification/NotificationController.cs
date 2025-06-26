using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Presentation.Controllers.Notification;

[ApiController]
[Route("api/v1/notifications")]
[ApiVersion("1.0")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int size = 50)
    {
        var result = await _notificationService.GetUserNotificationsAsync(page, size);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Notifications fetched successfully", data = result });
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        var result = await _notificationService.GetUnreadNotificationsAsync();
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Unread notifications fetched successfully", data = result });
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _notificationService.GetUnreadCountAsync();
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Unread count fetched successfully", data = result });
    }

    [HttpPost("{id}/mark-read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await _notificationService.MarkAsReadAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok();
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await _notificationService.MarkAllAsReadAsync();
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "All notifications marked as read successfully", data = result });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var result = await _notificationService.DeleteNotificationAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Notification deleted successfully", data = result });
    }
} 