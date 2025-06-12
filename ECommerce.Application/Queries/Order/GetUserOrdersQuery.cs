using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

public class GetUserOrdersQuery : IRequest<Result<List<OrderResponseDto>>>{}

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, Result<List<OrderResponseDto>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountRepository _accountRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILoggingService _logger;

    public GetUserOrdersQueryHandler(
        ICurrentUserService currentUserService,
        IAccountRepository accountRepository,
        IOrderRepository orderRepository,
        ILoggingService logger)
    {
        _currentUserService = currentUserService;
        _accountRepository = accountRepository;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result<List<OrderResponseDto>>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var accountResult = await GetCurrentUserAccountAsync();
            if (accountResult.IsFailure)
                return Result<List<OrderResponseDto>>.Failure(accountResult.Error);

            var userOrders = await _orderRepository.GetAccountOrders(accountResult.Data.Id);
            if (userOrders.Count == 0)
            {
                _logger.LogWarning("No orders found for this user: {Email}", accountResult.Data.Email);
                return Result<List<OrderResponseDto>>.Failure("No orders found for this user");
            }

            //TODO : Filter out orders that have purchased items in DB context
            var purchasedOrders = userOrders.Where(o => o.BasketItems.Any(oi => oi.IsOrdered)).ToList();

            var items = purchasedOrders.Select(order => new OrderResponseDto
            {
                AccountId = order.AccountId,
                BasketItems = order.BasketItems.Select(oi => new BasketItemResponseDto
                {
                    AccountId = oi.AccountId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ProductName = oi.ProductName
                }).ToList(),
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                Status = order.Status
            }).ToList();

            return Result<List<OrderResponseDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching user orders: {Message}", ex.Message);
            return Result<List<OrderResponseDto>>.Failure("An unexpected error occurred");
        }
    }

    private async Task<Result<ECommerce.Domain.Model.Account>> GetCurrentUserAccountAsync()
    {
        var emailResult = _currentUserService.GetCurrentUserEmail();
        if (emailResult is { IsSuccess: false, Error: not null })
        {
            _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
            return Result<ECommerce.Domain.Model.Account>.Failure(emailResult.Error);
        }
        
        if (emailResult.Data == null)
        {
            _logger.LogWarning("User email is null or empty");
            return Result<ECommerce.Domain.Model.Account>.Failure("User email is null or empty");
        }
        
        var account = await _accountRepository.GetAccountByEmail(emailResult.Data);
        if (account == null)
        {
            _logger.LogWarning("Account not found: {Email}", emailResult.Data);
            return Result<ECommerce.Domain.Model.Account>.Failure("Account not found");
        }

        return Result<ECommerce.Domain.Model.Account>.Success(account);
    }
}