using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
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
            if (accountResult.IsFailure && accountResult.Message is not null)
                return Result<List<OrderResponseDto>>.Failure(accountResult.Message);

            if (accountResult.Data is not null)
            {
                var userOrders = await GetUserOrders(accountResult.Data.Id);
                if (userOrders.Count == 0)
                    return Result<List<OrderResponseDto>>.Failure(ErrorMessages.OrderNotFound);

                var orderDtos = userOrders.Select(MapToResponseDto).ToList();
                return Result<List<OrderResponseDto>>.Success(orderDtos);
            }

            return Result<List<OrderResponseDto>>.Failure(ErrorMessages.AccountNotFound);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result<List<OrderResponseDto>>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<Result<Domain.Model.User>> GetCurrentUserAccountAsync()
    {
        var email = _currentUserService.GetUserEmail();        
        if (string.IsNullOrEmpty(email))
            return Result<Domain.Model.User>.Failure(ErrorMessages.AccountEmailNotFound);
        
        var account = await _accountRepository.GetAccountByEmail(email);
        if (account == null)
            return Result<Domain.Model.User>.Failure(ErrorMessages.AccountNotFound);

        return Result<Domain.Model.User>.Success(account);
    }

    private async Task<List<Domain.Model.Order>> GetUserOrders(string userId)
    {
        var orders = await _orderRepository.GetAccountOrders(userId);
        if (orders == null || orders.Count == 0)
            return new List<Domain.Model.Order>();

        return orders;
    }

    private static OrderResponseDto MapToResponseDto(Domain.Model.Order order) => new OrderResponseDto
    {
        UserId = order.UserId,
        BasketItems = order.BasketItems.Select(MapToBasketItemDto).ToList(),
        OrderDate = order.OrderDate,
        ShippingAddress = order.ShippingAddress,
        BillingAddress = order.BillingAddress,
        Status = order.Status
    };

    private static BasketItemResponseDto MapToBasketItemDto(Domain.Model.BasketItem basketItem) => new BasketItemResponseDto
    {
        UserId = basketItem.UserId,
        ProductId = basketItem.ProductId,
        Quantity = basketItem.Quantity,
        UnitPrice = basketItem.UnitPrice,
        ProductName = basketItem.ProductName
    };
}