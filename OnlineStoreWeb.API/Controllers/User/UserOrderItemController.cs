using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.Request.OrderItem;
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Forbid("User identity not found");
            }

            var userEmail = userIdClaim.Value;
            var orderItems = await _orderItemService.GetAllOrderItemsAsync(userEmail);
            return Ok(new { message = "Order items fetched successfully", data = orderItems });
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Forbid("User identity not found");
            }

            var userEmail = userIdClaim.Value;

        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
            
        try
        {
            await _orderItemService.CreateOrderItemAsync(orderItemCreateRequest, userEmail);
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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Forbid("User identity not found");
        }

        var userEmail = userIdClaim.Value;

        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            await _orderItemService.UpdateOrderItemAsync(orderItemUpdateRequest, userEmail);
            return Ok(new { message = "Order item updated successfully"});
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    
    [Authorize(Roles = "User")]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAllOrderItems()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Forbid("User identity not found");
            }

            var userEmail = userIdClaim.Value;

            await _orderItemService.DeleteAllOrderItemsAsync(userEmail);
            return Ok(new { message = "Order item deleted successfully"});
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}