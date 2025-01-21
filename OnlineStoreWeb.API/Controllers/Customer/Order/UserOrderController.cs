using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.Customer.Order;

[ApiController]
[Route("api/customer/orders")]
public class UserOrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public UserOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderCreateDto orderCreateRequest)
    {
        try
        {
            await _orderService.AddOrderAsync(orderCreateRequest);
            return Created($"orders", new { message = "Order created successfully" });
        }
        catch 
        {
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
        catch 
        {
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
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while deleting the order");
        }
    }
}