using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
namespace ECommerce.Application.Abstract.Service;

public interface IOrderService
{
    Task<Result> CreateOrderAsync(OrderCreateRequestDto orderCreateRequestDto);
    Task<Result> UpdateOrderStatusByAccountIdAsync(int accountId, OrderUpdateRequestDto orderUpdateRequestDto);
}