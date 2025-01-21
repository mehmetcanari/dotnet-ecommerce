using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.User.OrderItem;

[ApiController]
[Route("api/user/orderitems")]
public class UserOrderItemController : ControllerBase
{
    private readonly IOrderItemService _orderItemService;

    public UserOrderItemController(IOrderItemService orderItemService)
    {
        _orderItemService = orderItemService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrderItem(CreateOrderItemDto orderItemCreateRequest)
    {
        try
        {
            await _orderItemService.AddOrderItemAsync(orderItemCreateRequest);
            return Created($"orderitems/{orderItemCreateRequest.ProductId}", new { message = "Order item created successfully" });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while creating the order item");
        }
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateOrderItem(UpdateOrderItemDto updateOrderItemRequest)
    {
        try
        {
            await _orderItemService.UpdateOrderItemAsync(updateOrderItemRequest);
            return Ok(new { message = "Order item updated successfully" });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while updating the order item");
        }
    }

    [HttpGet("{userId}/{orderItemId}")]
    public async Task<IActionResult> GetSpecifiedUserOrderItem(int userId, int orderItemId)
    {
        try
        {
            var orderItem = await _orderItemService.GetSpecifiedOrderItemsWithUserIdAsync(userId, orderItemId);
            return Ok(new { message = "Order item fetched successfully", data = orderItem });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while fetching the order item");
        }
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetAllUserOrderItems(int userId)
    {
        try
        {
            var orderItems = await _orderItemService.GetAllOrderItemsWithUserIdAsync(userId);
            return Ok(new { message = "Order items fetched successfully", data = orderItems });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while fetching the order items");
        }
    }

    [HttpDelete("delete/{userId}/{orderItemId}")]
    public async Task<IActionResult> DeleteSpecifiedUserOrderItem(int userId, int orderItemId)
    {
        try
        {
            await _orderItemService.DeleteSpecifiedUserOrderItemAsync(userId, orderItemId);
            return Ok(new { message = "Order item deleted successfully" });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while deleting the order item");
        }
    }

    [HttpDelete("delete/{userId}")]
    public async Task<IActionResult> DeleteAllUserOrderItems(int userId)
    {
        try
        {
            await _orderItemService.DeleteAllUserOrderItemsAsync(userId);
            return Ok(new { message = "All order items deleted successfully" });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while deleting all order items");
        }
    }
}