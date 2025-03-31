using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.Request.OrderItem;
using OnlineStoreWeb.API.Model;
using OnlineStoreWeb.API.Services.OrderItem;

namespace OnlineStoreWeb.API.Controllers.User;

[ApiController]
[Route("api/user/order-items")]
public class UserOrderItemController : ControllerBase
{
    private readonly IOrderItemService _orderItemService;
    
    public UserOrderItemController(IOrderItemService orderItemService)
    {
        _orderItemService = orderItemService;
    }
    
    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<IActionResult> GetAllOrderItems()
    {
        try
        {
            var orderItems = await _orderItemService.GetAllOrderItemsAsync();
            return Ok(orderItems);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    
    [Authorize(Roles = "User")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrderItem([FromBody] CreateOrderItemDto orderItemCreateRequest)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
            
        try
        {
            await _orderItemService.CreateOrderItemAsync(orderItemCreateRequest);
            return Created($"order-items", new { message = "Order item created successfully"});
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    
    [Authorize(Roles = "User")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateOrderItem([FromBody] UpdateOrderItemDto orderItemUpdateRequest)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            await _orderItemService.UpdateOrderItemAsync(orderItemUpdateRequest);
            return Ok(new { message = "Order item updated successfully"});
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    
    [Authorize(Roles = "User")]
    [HttpDelete("delete/{accountId}")]
    public async Task<IActionResult> DeleteOrderItem([FromRoute] int accountId)
    {
        try
        {
            await _orderItemService.DeleteAllOrderItemsByAccountIdAsync(accountId);
            return Ok(new { message = "Order item deleted successfully"});
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}