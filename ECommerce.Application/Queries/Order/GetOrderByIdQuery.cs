using ECommerce.Application.Abstract;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.DTO.Response.BasketItem;
using ECommerce.Shared.DTO.Response.Order;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Queries.Order;

public class GetOrderByIdQuery(Guid id) : IRequest<Result<OrderResponseDto>>
{
    public readonly Guid Id = id;
}

public class GetOrderByIdQueryHandler(IOrderRepository orderRepository, ILogService logger) : IRequestHandler<GetOrderByIdQuery, Result<OrderResponseDto>>
{
    public async Task<Result<OrderResponseDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetById(request.Id, cancellationToken);
            if (order == null)
                return Result<OrderResponseDto>.Failure(ErrorMessages.OrderNotFound);

            var orderResponseDto = MapToResponseDto(order);
            return Result<OrderResponseDto>.Success(orderResponseDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.OrderNotFound, ex.Message);
            return Result<OrderResponseDto>.Failure(ErrorMessages.UnexpectedError);
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