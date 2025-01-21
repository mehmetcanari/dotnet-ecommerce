using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.Order;
using OnlineStoreWeb.API.Services.Order;

namespace OnlineStoreWeb.API.Controllers.Customer.Order;

[ApiController]
[Route("api/customer/orders")]
public class UserOrderController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderCreateDto orderCreateRequest)
    {
        try
        {
            await orderService.AddOrderAsync(orderCreateRequest);
            return Created($"orders", new { message = "Order created successfully"});
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
            var orders = await orderService.GetOrdersByUserIdAsync(userId);
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
            await orderService.DeleteOrderWithUserIdAsync(userId);
            return Ok(new { message = "Order deleted successfully" });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while deleting the order");
        }
    }
}