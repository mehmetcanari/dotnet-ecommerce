using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;

namespace ECommerce.Application.Services.Notification;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggingService _logger;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        ILoggingService logger,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Domain.Model.Notification>> CreateNotificationAsync(string title, string message, NotificationType type, string? relatedEntityId = null, string? relatedEntityType = null)
    {
        try
        {
            var userId = await GetCurrentUserId();
            if (userId.IsFailure || string.IsNullOrEmpty(userId.Data))
                return Result<Domain.Model.Notification>.Failure("User ID not found");

            var notification = new Domain.Model.Notification
            {
                UserId = userId.Data,
                Title = title,
                Message = message,
                Type = type,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType
            };

            await _notificationRepository.CreateAsync(notification);
            await _unitOfWork.Commit();
            _logger.LogInformation("Created notification {NotificationId} for user {UserId}", 
                notification.Id, userId.Data);

            return Result<Domain.Model.Notification>.Success(notification);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while creating notification", exception);
            return Result<Domain.Model.Notification>.Failure(exception.Message);
        }
    }

    public async Task<Result<IEnumerable<Domain.Model.Notification>>> GetUserNotificationsAsync(int page = 1, int size = 50)
    {
        try
        {
            var userId = await GetCurrentUserId();
            if (userId.IsFailure || string.IsNullOrEmpty(userId.Data))
                return Result<IEnumerable<Domain.Model.Notification>>.Failure("User ID not found");

            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId.Data, page, size);
            if (notifications == null || !notifications.Any())
                return Result<IEnumerable<Domain.Model.Notification>>.Failure("Notifications not found");

            return Result<IEnumerable<Domain.Model.Notification>>.Success(notifications);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while getting user notifications", exception);
            return Result<IEnumerable<Domain.Model.Notification>>.Failure(exception.Message);
        }
    }

    public async Task<Result<IEnumerable<Domain.Model.Notification>>> GetUnreadNotificationsAsync()
    {
        try
        {
            var userId = await GetCurrentUserId();
            if (userId.IsFailure || string.IsNullOrEmpty(userId.Data))
                return Result<IEnumerable<Domain.Model.Notification>>.Failure("User ID not found");

            var notifications = await _notificationRepository.GetUnreadNotificationsAsync(userId.Data);
            if (notifications == null || !notifications.Any())
                return Result<IEnumerable<Domain.Model.Notification>>.Failure("Notifications not found");

            return Result<IEnumerable<Domain.Model.Notification>>.Success(notifications);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while getting unread notifications", exception);
            return Result<IEnumerable<Domain.Model.Notification>>.Failure(exception.Message);
        }
    }

    public async Task<Result<int>> GetUnreadCountAsync()
    {
        try
        {
            var userId = await GetCurrentUserId();
            if (userId.IsFailure || string.IsNullOrEmpty(userId.Data))
                return Result<int>.Failure("User ID not found");

            var count = await _notificationRepository.GetUnreadCountAsync(userId.Data);
            if (count == 0)
                return Result<int>.Failure("No unread notifications found");

            return Result<int>.Success(count);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while getting unread count", exception);
            return Result<int>.Failure(exception.Message);
        }
    }

    public async Task<Result<bool>> MarkAsReadAsync(int notificationId)
    {
        try
        {
            var result = await _notificationRepository.MarkAsReadAsync(notificationId);
            await _unitOfWork.Commit();
            if (result is false)
                return Result<bool>.Failure("Failed to mark notification as read");

            return Result<bool>.Success(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while marking notification as read", exception);
            return Result<bool>.Failure(exception.Message);
        }
    }

    public async Task<Result<bool>> MarkAllAsReadAsync()
    {
        try
        {
            var userId = await GetCurrentUserId();
            if (userId.IsFailure || string.IsNullOrEmpty(userId.Data))
                return Result<bool>.Failure("User ID not found");

            var result = await _notificationRepository.MarkAllAsReadAsync(userId.Data);
            await _unitOfWork.Commit();
            if (result is false)
                return Result<bool>.Failure("Failed to mark all notifications as read");

            return Result<bool>.Success(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while marking all notifications as read", exception);
            return Result<bool>.Failure(exception.Message);
        }
    }

    public async Task<Result<bool>> DeleteNotificationAsync(int notificationId)
    {
        try
        {
            var result = await _notificationRepository.DeleteAsync(notificationId);
            await _unitOfWork.Commit();
            if (result is false)
                return Result<bool>.Failure("Failed to delete notification");

            return Result<bool>.Success(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while deleting notification", exception);
            return Result<bool>.Failure(exception.Message);
        }
    }

    public async Task<Result<bool>> SendNotificationToUserAsync(string title, string message, NotificationType type, string? relatedEntityId = null, string? relatedEntityType = null)
    {
        try
        {
            var result = await CreateNotificationAsync(title, message, type, relatedEntityId, relatedEntityType);
            if (result.IsFailure || result.Data == null)
                return Result<bool>.Failure(result.Error ?? "Failed to create notification");

            _logger.LogInformation("Notification sent to user {UserId}", result.Data.UserId);
            return Result<bool>.Success(true);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while sending notification to user", exception);
            return Result<bool>.Failure(exception.Message);
        }
    }

    private async Task<Result<string>> GetCurrentUserId()
    {
        var userIdResult = await _currentUserService.GetCurrentUserId();
        if (userIdResult.IsFailure || string.IsNullOrEmpty(userIdResult.Data))
            return Result<string>.Failure("User ID not found");

        return Result<string>.Success(userIdResult.Data);
    }
} 