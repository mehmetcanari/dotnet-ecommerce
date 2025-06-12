using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

public class CancelOrderCommand : IRequest<Result> {}

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
            var emailResult = _currentUserService.GetCurrentUserEmail();
            if (emailResult is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
                return Result.Failure(emailResult.Error);
            }
            
            if (emailResult.Data == null)
            {
                _logger.LogWarning("User email is null or empty");
                return Result.Failure("User email is null or empty");
            }
            
            var tokenAccount = await _accountRepository.GetAccountByEmail(emailResult.Data);
            if (tokenAccount == null)
            {
                _logger.LogWarning("Account not found: {Email}", emailResult.Data);
                return Result.Failure("Account not found");
            }
            
            var pendingOrders = await _orderRepository.GetAccountPendingOrders(tokenAccount.Id);

            if (pendingOrders.Count == 0)
            {
                return Result.Failure("No pending orders found");
            }

            foreach (var order in pendingOrders)
            {
                order.Status = OrderStatus.Cancelled;
                _orderRepository.Update(order);
            }

            await _unitOfWork.Commit();

            _logger.LogInformation("Orders cancelled successfully. Count: {Count}", pendingOrders.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while cancelling orders: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}