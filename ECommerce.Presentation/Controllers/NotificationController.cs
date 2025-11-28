using ECommerce.Application.Abstract;
using ECommerce.Application.Services.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize]
public class NotificationController(INotificationService notificationService, ICurrentUserService currentUserService) : ApiBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int size = 50) => HandleResult(await notificationService.GetUserNotificationsAsync(page, size));

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications() => HandleResult(await notificationService.GetUnreadNotificationsAsync());

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount() => HandleResult(await notificationService.GetUnreadNotificationsCountAsync());

    [HttpPost("{id}/mark-read")]
    public async Task<IActionResult> MarkAsRead(Guid id) => HandleResult(await notificationService.MarkAsReadAsync(id));

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead() => HandleResult(await notificationService.MarkAllAsReadAsync());

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(Guid id) => HandleResult(await notificationService.DeleteNotificationAsync(id));

    [HttpGet("hub-status")]
    public IActionResult GetHubStatus()
    {
        try
        {
            var userId = currentUserService.GetUserId();
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