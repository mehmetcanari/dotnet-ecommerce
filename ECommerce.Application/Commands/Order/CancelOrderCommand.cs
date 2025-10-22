using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Order;

public class CancelOrderCommand : IRequest<Result> { }

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggingService _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(ICurrentUserService currentUserService, ILoggingService logger, IOrderRepository orderRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _logger = logger;
        _orderRepository = orderRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = GetValidatedUserEmail();
            if (emailResult is null)
                return Result.Failure(ErrorMessages.AccountEmailNotFound);

            var accountResult = await ValidateAndGetAccount(emailResult);
            if (accountResult.IsFailure && accountResult.Error is not null)
                return Result.Failure(accountResult.Error);

            if (accountResult.Data is null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            var ordersResult = await ValidateAndGetPendingOrders(accountResult.Data);
            if (ordersResult.IsFailure && ordersResult.Error is not null)
                return Result.Failure(ordersResult.Error);

            if (ordersResult.Data is null || !ordersResult.Data.Any())
                return Result.Failure(ErrorMessages.NoPendingOrders);

            await CancelOrders(ordersResult.Data);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ex.Message);
        }
    }

    private string GetValidatedUserEmail()
    {
        var emailResult = _currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(emailResult))
        {
            _logger.LogWarning(ErrorMessages.AccountEmailNotFound);
            return string.Empty;
        }

        return emailResult;
    }

    private async Task<Result<Domain.Model.Account>> ValidateAndGetAccount(string email)
    {
        var account = await _accountRepository.GetAccountByEmail(email);
        if (account == null)
        {
            _logger.LogWarning(ErrorMessages.AccountNotFound, email);
            return Result<Domain.Model.Account>.Failure(ErrorMessages.AccountNotFound);
        }

        return Result<Domain.Model.Account>.Success(account);
    }

    private async Task<Result<List<Domain.Model.Order>>> ValidateAndGetPendingOrders(Domain.Model.Account account)
    {
        var pendingOrders = await _orderRepository.GetAccountPendingOrders(account.Id);
        if (!pendingOrders.Any())
        {
            _logger.LogInformation(ErrorMessages.NoPendingOrders, account.Id);
            return Result<List<Domain.Model.Order>>.Failure(ErrorMessages.NoPendingOrders);
        }

        return Result<List<Domain.Model.Order>>.Success(pendingOrders);
    }

    private async Task CancelOrders(List<Domain.Model.Order> orders)
    {
        foreach (var order in orders)
        {
            order.Status = OrderStatus.Cancelled;
            _orderRepository.Update(order);
        }

        await _unitOfWork.Commit();
        _logger.LogInformation(ErrorMessages.OrderCancelled, orders.Count);
    }
}