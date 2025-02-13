using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.Order;
using OnlineStoreWeb.API.Services.Order;

namespace OnlineStoreWeb.API.Controllers.User;

[ApiController]
[Route("api/user/orders")]
public class UserOrderController(IOrderService orderService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder(OrderCreateDto orderCreateRequest)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
            
        try
        {
            await orderService.AddOrderAsync(orderCreateRequest);
            return Created($"orders", new { message = "Order created successfully"});
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        try
        {
            var order = await orderService.GetOrderWithIdAsync(id);
            return Ok(new { message = "Order fetched successfully", data = order });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteOrder(int userId)
    {
        try
        {
            await orderService.DeleteOrderAsync(userId);
            return Ok(new { message = "Order deleted successfully" });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}