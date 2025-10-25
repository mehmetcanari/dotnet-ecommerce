using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Order;

public class CancelOrderCommand : IRequest<Result> { }

public class CancelOrderCommandHandler(ICurrentUserService currentUserService, ILoggingService logger, IOrderRepository orderRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork) : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var email = GetValidatedUserEmail();
            if (string.IsNullOrEmpty(email))
                return Result.Failure(ErrorMessages.AccountEmailNotFound);

            var accountResult = await ValidateAndGetAccount(email);
            if (accountResult is { IsFailure: true, Message: not null })
                return Result.Failure(accountResult.Message);

            if (accountResult.Data is null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            var ordersResult = await ValidateAndGetPendingOrders(accountResult.Data);
            if (ordersResult is { IsFailure: true, Message: not null })
                return Result.Failure(ordersResult.Message);

            if (ordersResult.Data is null || !ordersResult.Data.Any())
                return Result.Failure(ErrorMessages.NoPendingOrders);

            await CancelOrders(ordersResult.Data);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ex.Message);
        }
    }

    private string GetValidatedUserEmail()
    {
        var emailResult = currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(emailResult))
        {
            logger.LogWarning(ErrorMessages.AccountEmailNotFound);
            return string.Empty;
        }

        return emailResult;
    }

    private async Task<Result<Domain.Model.User>> ValidateAndGetAccount(string email)
    {
        var account = await accountRepository.GetByEmail(email);
        if (account == null)
        {
            logger.LogWarning(ErrorMessages.AccountNotFound, email);
            return Result<Domain.Model.User>.Failure(ErrorMessages.AccountNotFound);
        }

        return Result<Domain.Model.User>.Success(account);
    }

    private async Task<Result<List<Domain.Model.Order>>> ValidateAndGetPendingOrders(Domain.Model.User account)
    {
        var pendingOrders = await orderRepository.GetPendings(account.Id);
        if (pendingOrders.Count == 0)
            return Result<List<Domain.Model.Order>>.Failure(ErrorMessages.NoPendingOrders);

        return Result<List<Domain.Model.Order>>.Success(pendingOrders);
    }

    private async Task CancelOrders(List<Domain.Model.Order> orders)
    {
        foreach (var order in orders)
        {
            order.Status = OrderStatus.Cancelled;
            orderRepository.Update(order);
        }

        await unitOfWork.Commit();
        logger.LogInformation(ErrorMessages.OrderCancelled, orders.Count);
    }
}