using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Notification;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Services.Notification;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IRealtimeNotificationHandler _realtimeNotificationHandler;
    private readonly ILoggingService _logger;
    private readonly IStoreUnitOfWork _storeUnitOfWork;
    private readonly IMediator _mediator;

    public NotificationService(
        INotificationRepository notificationRepository,
        IRealtimeNotificationHandler realtimeNotificationHandler,
        ILoggingService logger,
        IStoreUnitOfWork storeUnitOfWork,
        IMediator mediator)
    {
        _notificationRepository = notificationRepository;
        _realtimeNotificationHandler = realtimeNotificationHandler;
        _logger = logger;
        _storeUnitOfWork = storeUnitOfWork;
        _mediator = mediator;
    }

    public async Task<Result<Domain.Model.Notification>> CreateNotificationAsync(string title, string message, NotificationType type)
    {
        try
        {
            var accountResult = await _mediator.Send(new GetClientAccountAsEntityQuery());
            if (accountResult.IsFailure && accountResult.Message is not null)
                return Result<Domain.Model.Notification>.Failure(accountResult.Message);

            if(accountResult.Data == null)
                return Result<Domain.Model.Notification>.Failure(ErrorMessages.AccountNotFound);

            var notification = new Domain.Model.Notification
            {
                UserId = accountResult.Data.Id,
                Title = title,
                Message = message,
                Type = type,
            };

            await _notificationRepository.CreateAsync(notification);
            await _storeUnitOfWork.Commit();
            await _realtimeNotificationHandler.HandleNotification(title, message, type, accountResult.Data.Id);

            return Result<Domain.Model.Notification>.Success(notification);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorMessages.UnexpectedHubError, exception);
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
            var account = await _mediator.Send(new GetClientAccountAsEntityQuery());
            if (account.IsFailure && account.Message is not null)
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(account.Message);

            if(account.Data == null)
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(ErrorMessages.AccountNotFound);

            var notifications = await _notificationRepository.GetUserNotificationsAsync(account.Data.Id, page, size);
            if (notifications == null || !notifications.Any())
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(ErrorMessages.NotificationsNotFound);

            return Result<IEnumerable<Domain.Model.Notification>>.Success(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedHubError, ex);
            return Result<IEnumerable<Domain.Model.Notification>>.Failure(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<Domain.Model.Notification>>> GetUnreadNotificationsAsync()
    {
        try
        {
            var account = await _mediator.Send(new GetClientAccountAsEntityQuery());
            if (account.IsFailure && account.Message is not null)
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(account.Message);

            if (account.Data == null)
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(ErrorMessages.AccountNotFound);

            var notifications = await _notificationRepository.GetUnreadNotificationsAsync(account.Data.Id);
            if (notifications == null || !notifications.Any())
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(ErrorMessages.NotificationsNotFound);

            return Result<IEnumerable<Domain.Model.Notification>>.Success(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedHubError, ex);
            return Result<IEnumerable<Domain.Model.Notification>>.Failure(ex.Message);
        }
    }

    public async Task<Result<int>> GetUnreadNotificationsCountAsync()
    {
        try
        {
            var account = await _mediator.Send(new GetClientAccountAsEntityQuery());
            if (account.IsFailure && account.Message is not null)
                return Result<int>.Failure(account.Message);

            if (account.Data == null)
                return Result<int>.Failure(ErrorMessages.AccountNotFound);

            var count = await _notificationRepository.GetUnreadCountAsync(account.Data.Id.ToString());
            if (count == 0)
                return Result<int>.Failure(ErrorMessages.NoUnreadNotifications);

            return Result<int>.Success(count);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorMessages.UnexpectedHubError, exception);
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
                return Result<bool>.Failure(ErrorMessages.ErrorMarkingNotificationsAsRead);

            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedHubError, ex);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> MarkAllAsReadAsync()
    {
        try
        {
            var account = await _mediator.Send(new GetClientAccountAsEntityQuery());
            if (account.IsFailure && account.Message is not null)
                return Result<bool>.Failure(account.Message);

            if (account.Data == null)
                return Result<bool>.Failure(ErrorMessages.AccountNotFound);

            var result = await _notificationRepository.MarkAllAsReadAsync(account.Data.Id);
            if (result is false)
                return Result<bool>.Failure(ErrorMessages.ErrorMarkingNotificationsAsRead);

            await _storeUnitOfWork.Commit();

            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedHubError, ex);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteNotificationAsync(int notificationId)
    {
        try
        {
            var result = await _notificationRepository.DeleteAsync(notificationId);
            await _storeUnitOfWork.Commit();
            if (result is false)
                return Result<bool>.Failure(ErrorMessages.FailedToDeleteNotification);

            return Result<bool>.Success(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorMessages.UnexpectedHubError, exception);
            return Result<bool>.Failure(exception.Message);
        }
    }
} 