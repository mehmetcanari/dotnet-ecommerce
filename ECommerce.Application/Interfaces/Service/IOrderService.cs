using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Domain.Model;

namespace ECommerce.Application.Interfaces.Service;

public interface IOrderService
{
    Task<List<OrderResponseDto>> GetAllOrdersAsync();
    Task<List<OrderResponseDto>> GetUserOrdersAsync(string email);
    Task<OrderResponseDto> GetOrderByIdAsync(int id);
    Task AddOrderAsync(OrderCreateRequestDto orderCreateRequestDto, string email);
    Task CancelOrderAsync(string email);
    Task DeleteOrderByIdAsync(int id);
    Task UpdateOrderStatusByAccountIdAsync(int accountId, OrderUpdateRequestDto orderUpdateRequestDto);
}