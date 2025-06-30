using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Notification;
using ECommerce.Application.Queries.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using MediatR;

namespace ECommerce.Application.Services.Notification;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IRealtimeNotificationHandler _realtimeNotificationHandler;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggingService _logger;
    private readonly IStoreUnitOfWork _storeUnitOfWork;
    private readonly IMediator _mediator;

    public NotificationService(
        INotificationRepository notificationRepository,
        IRealtimeNotificationHandler realtimeNotificationHandler,
        ICurrentUserService currentUserService,
        ILoggingService logger,
        IStoreUnitOfWork storeUnitOfWork,
        IMediator mediator)
    {
        _notificationRepository = notificationRepository;
        _realtimeNotificationHandler = realtimeNotificationHandler;
        _currentUserService = currentUserService;
        _logger = logger;
        _storeUnitOfWork = storeUnitOfWork;
        _mediator = mediator;
    }

    public async Task<Result<Domain.Model.Notification>> CreateNotificationAsync(string title, string message, NotificationType type)
    {
        try
        {
            var account = await _mediator.Send(new GetClientAccountQuery());
            if (account.IsFailure)
                return Result<Domain.Model.Notification>.Failure(account.Error);

            var notification = new Domain.Model.Notification
            {
                AccountId = account.Data.Id,
                Title = title,
                Message = message,
                Type = type,
            };

            await _notificationRepository.CreateAsync(notification);
            await _storeUnitOfWork.Commit();
            await _realtimeNotificationHandler.HandleNotification(title, message, type, account.Data.Id.ToString());

            _logger.LogInformation("Created notification {NotificationId} for user {UserId}", 
                notification.Id, account.Data.Id);

            return Result<Domain.Model.Notification>.Success(notification);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while creating notification", exception);
            return Result<Domain.Model.Notification>.Failure(exception.Message);
        }
    }

    public async Task<Result<Domain.Model.Notification>> TestCreateNotificationAsync(SendNotificationRequestDto request)
    {
        return await CreateNotificationAsync(request.Title, request.Message, request.Type);
    }

    public async Task<Result<IEnumerable<Domain.Model.Notification>>> GetUserNotificationsAsync(int page = 1, int size = 50)
    {
        try
        {
            var account = await _mediator.Send(new GetClientAccountQuery());
            if (account.IsFailure)
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(account.Error);

            var notifications = await _notificationRepository.GetUserNotificationsAsync(account.Data.Id, page, size);
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
            var account = await _mediator.Send(new GetClientAccountQuery());
            if (account.IsFailure)
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(account.Error);

            var notifications = await _notificationRepository.GetUnreadNotificationsAsync(account.Data.Id);
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
            var account = await _mediator.Send(new GetClientAccountQuery());
            if (account.IsFailure)
                return Result<int>.Failure(account.Error);

            var count = await _notificationRepository.GetUnreadCountAsync(account.Data.Id.ToString());
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
            await _storeUnitOfWork.Commit();
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
            var account = await _mediator.Send(new GetClientAccountQuery());
            if (account.IsFailure)
                return Result<bool>.Failure(account.Error);

            var result = await _notificationRepository.MarkAllAsReadAsync(account.Data.Id);
            if (result is false)
                return Result<bool>.Failure("Failed to mark all notifications as read");

            _logger.LogInformation("Marked all notifications as read for user {UserId}", account.Data.Id);
            await _storeUnitOfWork.Commit();

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
            await _storeUnitOfWork.Commit();
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

    public Task<Result<bool>> SendNotificationToAllUsersAsync(string title, string message, NotificationType type)
    {
        throw new NotImplementedException();
    }
} 