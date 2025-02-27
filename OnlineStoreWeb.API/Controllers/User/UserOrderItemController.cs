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
    
    [HttpGet]
    public async Task<IActionResult> GetAllOrderItems()
    {
        try
        {
            IEnumerable<OrderItem> orderItems = await _orderItemService.GetAllOrderItemsAsync();
            return Ok(orderItems);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrderItem(CreateOrderItemDto orderItemCreateRequest)
    {
        // if(!ModelState.IsValid)
        // {
        //     return BadRequest(ModelState);
        // }
            
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
    
    [HttpPut("update")]
    public async Task<IActionResult> UpdateOrderItem(UpdateOrderItemDto orderItemUpdateRequest)
    {
        // if(!ModelState.IsValid)
        // {
        //     return BadRequest(ModelState);
        // }
        
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
    
    [HttpDelete("delete/{orderItemId}")]
    public async Task<IActionResult> DeleteOrderItem(int orderItemId)
    {
        try
        {
            await _orderItemService.DeleteOrderItemAsync(orderItemId);
            return Ok(new { message = "Order item deleted successfully"});
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}