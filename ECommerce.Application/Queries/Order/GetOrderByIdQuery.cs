using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Order;

public class GetOrderByIdQuery : IRequest<Result<OrderResponseDto>>
{
    public required Guid Id { get; set; }
}

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILoggingService _logger;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, ILoggingService logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result<OrderResponseDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetById(request.Id);
            if (order == null)
                return Result<OrderResponseDto>.Failure(ErrorMessages.OrderNotFound);

            var orderResponseDto = MapToResponseDto(order);
            return Result<OrderResponseDto>.Success(orderResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.OrderNotFound, ex.Message);
            return Result<OrderResponseDto>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private static OrderResponseDto MapToResponseDto(Domain.Model.Order order) => new OrderResponseDto
    {
        UserId = order.UserId,
        BasketItems = order.BasketItems.Select(MapToBasketItemDto).ToList(),
        OrderDate = order.CreatedOn,
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