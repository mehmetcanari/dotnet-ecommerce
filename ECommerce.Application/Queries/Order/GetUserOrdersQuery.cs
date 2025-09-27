using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Queries.Order;

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

            var userOrders = await GetUserOrders(accountResult.Data.Id);
            if (userOrders.Count == 0)
            {
                _logger.LogWarning("No orders found for this user: {Email}", accountResult.Data.Email);
                return Result<List<OrderResponseDto>>.Failure("No orders found for this user");
            }

            var orderDtos = userOrders.Select(MapToResponseDto).ToList();
            return Result<List<OrderResponseDto>>.Success(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching user orders: {Message}", ex.Message);
            return Result<List<OrderResponseDto>>.Failure("An unexpected error occurred");
        }
    }

    private async Task<Result<Domain.Model.Account>> GetCurrentUserAccountAsync()
    {
        var emailResult = _currentUserService.GetUserEmail();
        if (emailResult is { IsSuccess: false, Error: not null })
        {
            _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
            return Result<Domain.Model.Account>.Failure(emailResult.Error);
        }
        
        if (emailResult.Data == null)
        {
            _logger.LogWarning("User email is null or empty");
            return Result<Domain.Model.Account>.Failure("User email is null or empty");
        }
        
        var account = await _accountRepository.GetAccountByEmail(emailResult.Data);
        if (account == null)
        {
            _logger.LogWarning("Account not found: {Email}", emailResult.Data);
            return Result<Domain.Model.Account>.Failure("Account not found");
        }

        return Result<Domain.Model.Account>.Success(account);
    }

    private async Task<List<Domain.Model.Order>> GetUserOrders(int accountId)
    {
        var orders = await _orderRepository.GetAccountOrders(accountId);
        if (orders == null || orders.Count == 0)
        {
            _logger.LogWarning("No orders found for account ID: {AccountId}", accountId);
            return new List<Domain.Model.Order>();
        }

        return orders;
    }

    private static OrderResponseDto MapToResponseDto(Domain.Model.Order order)
    {
        return new OrderResponseDto
        {
            AccountId = order.AccountId,
            BasketItems = order.BasketItems.Select(MapToBasketItemDto).ToList(),
            OrderDate = order.OrderDate,
            ShippingAddress = order.ShippingAddress,
            BillingAddress = order.BillingAddress,
            Status = order.Status
        };
    }

    private static BasketItemResponseDto MapToBasketItemDto(Domain.Model.BasketItem basketItem)
    {
        return new BasketItemResponseDto
        {
            AccountId = basketItem.AccountId,
            ProductId = basketItem.ProductId,
            Quantity = basketItem.Quantity,
            UnitPrice = basketItem.UnitPrice,
            ProductName = basketItem.ProductName
        };
    }
}