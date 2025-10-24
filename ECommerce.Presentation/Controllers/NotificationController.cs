using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Services.Notification;
using ECommerce.Application.Validations.Attribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize]
public class NotificationController(INotificationService _notificationService, ICurrentUserService _currentUserService) : ApiBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int size = 50) => HandleResult(await _notificationService.GetUserNotificationsAsync(page, size));

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications() => HandleResult(await _notificationService.GetUnreadNotificationsAsync());

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount() => HandleResult(await _notificationService.GetUnreadNotificationsCountAsync());

    [ValidateId]
    [HttpPost("{id}/mark-read")]
    [ValidateId]
    public async Task<IActionResult> MarkAsRead(Guid id) => HandleResult(await _notificationService.MarkAsReadAsync(id));

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead() => HandleResult(await _notificationService.MarkAllAsReadAsync());

    [ValidateId]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(Guid id) => HandleResult(await _notificationService.DeleteNotificationAsync(id));

    [HttpGet("hub-status")]
    public IActionResult GetHubStatus()
    {
        try
        {
            var userId = _currentUserService.GetUserId();            
            var isConnected = NotificationHub.IsUserConnected(userId);
            var connectionId = NotificationHub.GetUserConnectionId(userId);
            var totalConnections = NotificationHub.GetTotalConnections();

            return Ok(new 
            { 
                message = "Hub status retrieved successfully", 
                data = new
                {
                    hubConnected = isConnected,
                    connectionId,
                    userId,
                    totalConnections
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error checking hub status: {ex.Message}", hubConnected = false });
        }
    }
} 