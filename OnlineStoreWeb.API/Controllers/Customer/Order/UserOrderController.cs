using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.Customer.Order;

[ApiController]
[Route("api/customer/orders")]
public class UserOrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<UserOrderController> _logger;

    public UserOrderController(IOrderRepository orderRepository, ILogger<UserOrderController> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderCreateDto orderCreateRequest)
    {
        try
        {
            await _orderRepository.AddOrderAsync(orderCreateRequest);
            return Created($"orders", new { message = "Order created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating order: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while creating the order");
        }
    }

    [HttpGet("userId")]
    public async Task<IActionResult> GetOrdersByUserId(int userId)
    {
        try
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return Ok(new { message = "Orders fetched successfully", orders });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching orders: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while fetching orders");
        }
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteOrder(int userId)
    {
        try
        {
            await _orderRepository.DeleteOrderWithUserIdAsync(userId);
            return Ok(new { message = "Order deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while deleting the order");
        }
    }
}