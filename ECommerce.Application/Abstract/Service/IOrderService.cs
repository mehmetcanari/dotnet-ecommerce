using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
namespace ECommerce.Application.Abstract.Service;

public interface IOrderService
{
    Task<Result<List<OrderResponseDto>>> GetAllOrdersAsync();
    Task<Result<List<OrderResponseDto>>> GetUserOrdersAsync();
    Task<Result<OrderResponseDto>> GetOrderByIdAsync(int id);
    Task<Result> CreateOrderAsync(OrderCreateRequestDto orderCreateRequestDto);
    Task<Result> CancelOrderAsync();
    Task<Result> DeleteOrderByIdAsync(int id);
    Task<Result> UpdateOrderStatusByAccountIdAsync(int accountId, OrderUpdateRequestDto orderUpdateRequestDto);
}