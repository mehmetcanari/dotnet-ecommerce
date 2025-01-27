using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
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
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while creating the order");
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
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while fetching the order");
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
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while deleting the order");
        }
    }
}