using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Order;

public class GetAllOrdersQuery : IRequest<Result<List<OrderResponseDto>>>{}

public class GetAllOrdersQueryHandler(IOrderRepository orderRepository, ILogService logger) : IRequestHandler<GetAllOrdersQuery, Result<List<OrderResponseDto>>>
{
    public async Task<Result<List<OrderResponseDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await orderRepository.Read(cancellationToken: cancellationToken);
            if (orders.Count == 0)
                return Result<List<OrderResponseDto>>.Failure(ErrorMessages.OrderNotFound);

            var ordersResponse = orders.Select(MapToResponseDto).ToList();
            return Result<List<OrderResponseDto>>.Success(ordersResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.OrderNotFound, ex.Message);
            return Result<List<OrderResponseDto>>.Failure(ErrorMessages.UnexpectedError);
        }
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

    private BasketItemResponseDto MapToBasketItemDto(Domain.Model.BasketItem basketItem) => new BasketItemResponseDto
    {
        UserId = basketItem.UserId,
        ProductId = basketItem.ProductId,
        Quantity = basketItem.Quantity,
        UnitPrice = basketItem.UnitPrice,
        ProductName = basketItem.ProductName
    };
}