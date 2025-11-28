using ECommerce.Application.Abstract;
using ECommerce.Application.Queries.Account;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Services.Notification;

public class NotificationService(INotificationRepository notificationRepository, IRealtimeNotificationHandler realtimeNotificationHandler, ILogService logger, IStoreUnitOfWork storeUnitOfWork, IMediator mediator) : INotificationService
{
    public async Task<Result<Domain.Model.Notification>> CreateNotificationAsync(string title, string message, NotificationType type)
    {
        try
        {
            var accountResult = await mediator.Send(new GetClientAccountAsEntityQuery());
            if (accountResult.IsFailure && accountResult.Message is not null)
                return Result<Domain.Model.Notification>.Failure(accountResult.Message);

            if (accountResult.Data == null)
                return Result<Domain.Model.Notification>.Failure(ErrorMessages.AccountNotFound);

            var notification = new Domain.Model.Notification
            {
                UserId = accountResult.Data.Id,
                Title = title,
                Message = message,
                Type = type,
            };

            await notificationRepository.CreateAsync(notification);
            await storeUnitOfWork.Commit();
            await realtimeNotificationHandler.HandleNotification(title, message, type, accountResult.Data.Id);

            return Result<Domain.Model.Notification>.Success(notification);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ErrorMessages.UnexpectedHubError, exception);
            return Result<Domain.Model.Notification>.Failure(exception.Message);
        }
    }

    public async Task<Result<IEnumerable<Domain.Model.Notification>>> GetUserNotificationsAsync(int page = 1, int size = 50)
    {
        try
        {
            var account = await mediator.Send(new GetClientAccountAsEntityQuery());
            if (account.IsFailure && account.Message is not null)
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(account.Message);

            if (account.Data == null)
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(ErrorMessages.AccountNotFound);

            var notifications = await notificationRepository.GetAsync(account.Data.Id, page, size);
            var enumerable = notifications as Domain.Model.Notification[] ?? notifications.ToArray();
            if (!enumerable.Any())
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(ErrorMessages.NotificationsNotFound);

            return Result<IEnumerable<Domain.Model.Notification>>.Success(enumerable);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedHubError, ex);
            return Result<IEnumerable<Domain.Model.Notification>>.Failure(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<Domain.Model.Notification>>> GetUnreadNotificationsAsync()
    {
        try
        {
            var account = await mediator.Send(new GetClientAccountAsEntityQuery());
            if (account.IsFailure && account.Message is not null)
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(account.Message);

            if (account.Data == null)
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(ErrorMessages.AccountNotFound);

            var notifications = await notificationRepository.GetUnreadAsync(account.Data.Id);
            if (!notifications.Any())
                return Result<IEnumerable<Domain.Model.Notification>>.Failure(ErrorMessages.NotificationsNotFound);

            return Result<IEnumerable<Domain.Model.Notification>>.Success(notifications);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedHubError, ex);
            return Result<IEnumerable<Domain.Model.Notification>>.Failure(ex.Message);
        }
    }

    public async Task<Result<int>> GetUnreadNotificationsCountAsync()
    {
        try
        {
            var account = await mediator.Send(new GetClientAccountAsEntityQuery());
            if (account is { IsFailure: true, Message: not null })
                return Result<int>.Failure(account.Message);

            if (account.Data == null)
                return Result<int>.Failure(ErrorMessages.AccountNotFound);

            var count = await notificationRepository.GetUnreadCountAsync(account.Data.Id);
            if (count == 0)
                return Result<int>.Failure(ErrorMessages.NoUnreadNotifications);

            return Result<int>.Success(count);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ErrorMessages.UnexpectedHubError, exception);
            return Result<int>.Failure(exception.Message);
        }
    }

    public async Task<Result<bool>> MarkAsReadAsync(Guid notificationId)
    {
        try
        {
            var result = await notificationRepository.MarkAsReadAsync(notificationId);
            await storeUnitOfWork.Commit();
            if (result is false)
                return Result<bool>.Failure(ErrorMessages.ErrorMarkingNotificationsAsRead);

            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedHubError, ex);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> MarkAllAsReadAsync()
    {
        try
        {
            var account = await mediator.Send(new GetClientAccountAsEntityQuery());
            if (account.IsFailure && account.Message is not null)
                return Result<bool>.Failure(account.Message);

            if (account.Data == null)
                return Result<bool>.Failure(ErrorMessages.AccountNotFound);

            var result = await notificationRepository.MarkAllAsReadAsync(account.Data.Id);
            if (result is false)
                return Result<bool>.Failure(ErrorMessages.ErrorMarkingNotificationsAsRead);

            await storeUnitOfWork.Commit();

            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedHubError, ex);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteNotificationAsync(Guid notificationId)
    {
        try
        {
            notificationRepository.Delete(notificationId);
            await storeUnitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ErrorMessages.UnexpectedHubError, exception);
            return Result.Failure(exception.Message);
        }
    }
}