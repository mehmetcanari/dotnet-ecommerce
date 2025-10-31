using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Order;

public class GetAllOrdersQuery : IRequest<Result<List<OrderResponseDto>>>;

public class GetAllOrdersQueryHandler(IOrderRepository orderRepository, ILogService logger, ICacheService cache) : IRequestHandler<GetAllOrdersQuery, Result<List<OrderResponseDto>>>
{
    private readonly TimeSpan _expiration = TimeSpan.FromMinutes(30);

    public async Task<Result<List<OrderResponseDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cacheItems = await cache.GetAsync<List<Domain.Model.Order>>(CacheKeys.Orders, cancellationToken);
            if (cacheItems is { Count: > 0 })
            {
                var cachedResponse = cacheItems.Select(MapToResponseDto).ToList();
                return Result<List<OrderResponseDto>>.Success(cachedResponse);
            }

            var orders = await orderRepository.Read(cancellationToken: cancellationToken);
            if (orders.Count == 0)
                return Result<List<OrderResponseDto>>.Failure(ErrorMessages.OrderNotFound);

            var response = orders.Select(MapToResponseDto).ToList();
            await cache.SetAsync(CacheKeys.Orders, orders, CacheExpirationType.Absolute, _expiration, cancellationToken);

            return Result<List<OrderResponseDto>>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.OrderNotFound, ex.Message);
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