using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
namespace ECommerce.Application.Abstract.Service;

public interface IOrderService
{
    Task<Result<List<OrderResponseDto>>> GetAllOrdersAsync();
    Task<Result<List<OrderResponseDto>>> GetUserOrdersAsync(string email);
    Task<Result<OrderResponseDto>> GetOrderByIdAsync(int id);
    Task AddOrderAsync(OrderCreateRequestDto orderCreateRequestDto, string email);
    Task CancelOrderAsync(string email);
    Task DeleteOrderByIdAsync(int id);
    Task UpdateOrderStatusByAccountIdAsync(int accountId, OrderUpdateRequestDto orderUpdateRequestDto);
}