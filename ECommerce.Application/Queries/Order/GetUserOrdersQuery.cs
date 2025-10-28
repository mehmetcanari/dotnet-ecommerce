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

public class GetUserOrdersQueryHandler(ICurrentUserService currentUserService, IOrderRepository orderRepository, ILogService logger) : IRequestHandler<GetUserOrdersQuery, Result<List<OrderResponseDto>>>
{
    public async Task<Result<List<OrderResponseDto>>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result<List<OrderResponseDto>>.Failure(ErrorMessages.AccountNotAuthorized);

            var orders = await orderRepository.GetClientCompletedOrders(Guid.Parse(userId), cancellationToken);
            if (orders.Count == 0)
                return Result<List<OrderResponseDto>>.Failure(ErrorMessages.OrderNotFound);

            var ordersResponse = orders.Select(MapToResponseDto).ToList();
            return Result<List<OrderResponseDto>>.Success(ordersResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result<List<OrderResponseDto>>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private OrderResponseDto MapToResponseDto(Domain.Model.Order order) => new()
    {
        UserId = order.UserId,
        BasketItems = order.BasketItems.Select(MapToBasketItemDto).ToList(),
        OrderDate = order.CreatedOn,
        ShippingAddress = order.ShippingAddress,
        BillingAddress = order.BillingAddress,
        Status = order.Status
    };

    private BasketItemResponseDto MapToBasketItemDto(BasketItem basketItem) => new()
    {
        UserId = basketItem.UserId,
        ProductId = basketItem.ProductId,
        Quantity = basketItem.Quantity,
        UnitPrice = basketItem.UnitPrice,
        ProductName = basketItem.ProductName
    };
}