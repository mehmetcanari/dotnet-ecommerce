using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.Request.Order;
using OnlineStoreWeb.API.Services.Order;

namespace OnlineStoreWeb.API.Controllers.User;

[ApiController]
[Route("api/user/orders")]
public class UserOrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public UserOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize(Roles = "User")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto orderCreateRequest)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
            
        try
        {
            await _orderService.AddOrderAsync(orderCreateRequest);
            return Created($"orders", new { message = "Order created successfully"});
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    
    [Authorize(Roles = "User")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById([FromRoute] int id)
    {
        try
        {
            var order = await _orderService.GetOrderWithIdAsync(id);
            return Ok(new { message = "Order fetched successfully", data = order });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
        
    [Authorize(Roles = "User")]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteOrder([FromRoute] int userId)
    {
        try
        {
            await _orderService.DeleteOrderByAccountIdAsync(userId);
            return Ok(new { message = "Order deleted successfully" });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}