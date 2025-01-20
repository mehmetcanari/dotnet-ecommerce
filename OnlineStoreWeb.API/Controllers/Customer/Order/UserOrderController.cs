using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.Customer.Order;

[ApiController]
[Route("api/customer/orders")]
public class UserOrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<UserOrderController> _logger;

    public UserOrderController(IOrderService orderService, ILogger<UserOrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderCreateDto orderCreateRequest)
    {
        try
        {
            await _orderService.AddOrderAsync(orderCreateRequest);
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
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
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
            await _orderService.DeleteOrderWithUserIdAsync(userId);
            return Ok(new { message = "Order deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while deleting the order");
        }
    }
}