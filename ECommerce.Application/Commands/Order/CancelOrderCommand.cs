using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
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

    public CancelOrderCommandHandler(
        ICurrentUserService currentUserService,
        ILoggingService logger,
        IOrderRepository orderRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
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
            var emailResult = await GetValidatedUserEmail();
            if (emailResult.IsFailure)
                return Result.Failure(emailResult.Error);

            var accountResult = await ValidateAndGetAccount(emailResult.Data);
            if (accountResult.IsFailure)
                return Result.Failure(accountResult.Error);

            var ordersResult = await ValidateAndGetPendingOrders(accountResult.Data);
            if (ordersResult.IsFailure)
                return Result.Failure(ordersResult.Error);

            await CancelOrders(ordersResult.Data);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while cancelling orders: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result<string>> GetValidatedUserEmail()
    {
        var emailResult = await Task.FromResult(_currentUserService.GetUserEmail());
        if (emailResult.IsFailure)
        {
            _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
            return Result<string>.Failure(emailResult.Error);
        }

        return Result<string>.Success(emailResult.Data);
    }

    private async Task<Result<Domain.Model.Account>> ValidateAndGetAccount(string email)
    {
        var account = await _accountRepository.GetAccountByEmail(email);
        if (account == null)
        {
            _logger.LogWarning("Account not found: {Email}", email);
            return Result<Domain.Model.Account>.Failure("Account not found");
        }

        return Result<Domain.Model.Account>.Success(account);
    }

    private async Task<Result<List<Domain.Model.Order>>> ValidateAndGetPendingOrders(Domain.Model.Account account)
    {
        var pendingOrders = await _orderRepository.GetAccountPendingOrders(account.Id);
        if (!pendingOrders.Any())
        {
            _logger.LogInformation("No pending orders found for account: {AccountId}", account.Id);
            return Result<List<Domain.Model.Order>>.Failure("No pending orders found");
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
        _logger.LogInformation("Orders cancelled successfully. Count: {Count}", orders.Count);
    }
}