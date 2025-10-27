using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Order;

public class GetUserOrdersQuery : IRequest<Result<List<OrderResponseDto>>>{}

public class GetUserOrdersQueryHandler(ICurrentUserService currentUserService, IAccountRepository accountRepository, IOrderRepository orderRepository, ILogService logger) : IRequestHandler<GetUserOrdersQuery, Result<List<OrderResponseDto>>>
{
    public async Task<Result<List<OrderResponseDto>>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var accountResult = await GetCurrentUserAccountAsync();
            if (accountResult is { IsFailure: true, Message: not null })
                return Result<List<OrderResponseDto>>.Failure(ErrorMessages.AccountNotFound);

            if (accountResult.Data is not null)
            {
                var orders = await orderRepository.GetOrders(accountResult.Data.Id, cancellationToken);
                if (orders.Count == 0)
                    return Result<List<OrderResponseDto>>.Failure(ErrorMessages.OrderNotFound);

                var ordersResponse = orders.Select(MapToResponseDto).ToList();
                return Result<List<OrderResponseDto>>.Success(ordersResponse);
            }

            return Result<List<OrderResponseDto>>.Failure(ErrorMessages.AccountNotFound);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result<List<OrderResponseDto>>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<Result<User>> GetCurrentUserAccountAsync()
    {
        var email = currentUserService.GetUserEmail();        
        if (string.IsNullOrEmpty(email))
            return Result<User>.Failure(ErrorMessages.AccountEmailNotFound);
        
        var account = await accountRepository.GetByEmail(email);
        if (account == null)
            return Result<User>.Failure(ErrorMessages.AccountNotFound);

        return Result<User>.Success(account);
    }

    private OrderResponseDto MapToResponseDto(Domain.Model.Order order) => new OrderResponseDto
    {
        UserId = order.UserId,
        BasketItems = order.BasketItems.Select(MapToBasketItemDto).ToList(),
        OrderDate = order.CreatedOn,
        ShippingAddress = order.ShippingAddress,
        BillingAddress = order.BillingAddress,
        Status = order.Status
    };

    private BasketItemResponseDto MapToBasketItemDto(BasketItem basketItem) => new BasketItemResponseDto
    {
        UserId = basketItem.UserId,
        ProductId = basketItem.ProductId,
        Quantity = basketItem.Quantity,
        UnitPrice = basketItem.UnitPrice,
        ProductName = basketItem.ProductName
    };
}