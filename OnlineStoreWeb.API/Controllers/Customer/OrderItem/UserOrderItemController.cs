using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.User.OrderItem;

[ApiController]
[Route("api/user/orderitems")]
public class UserOrderItemController : ControllerBase
{
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ILogger<UserOrderItemController> _logger;

    public UserOrderItemController(IOrderItemRepository orderItemRepository, ILogger<UserOrderItemController> logger)
    {
        _orderItemRepository = orderItemRepository;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrderItem(CreateOrderItemDto orderItemCreateRequest)
    {
        try
        {
            if (orderItemCreateRequest == null)
                return BadRequest(new { message = "Order item data is required" });

            await _orderItemRepository.AddOrderItemAsync(orderItemCreateRequest);
            return Created($"orderitems/{orderItemCreateRequest.ProductId}", new { message = "Order item created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating order item: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while creating the order item");
        }
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateOrderItem(int id, UpdateOrderItemDto updateOrderItemRequest)
    {
        try
        {
            await _orderItemRepository.UpdateOrderItemAsync(id, updateOrderItemRequest);
            return Ok(new { message = "Order item updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating order item: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while updating the order item");
        }
    }

    [HttpGet("{userId}/{orderItemId}")]
    public async Task<IActionResult> GetSpecifiedUserOrderItem(int userId, int orderItemId)
    {
        try
        {
            var orderItem = await _orderItemRepository.GetSpecifiedOrderItemsWithUserIdAsync(userId, orderItemId);
            return Ok(new { message = "Order item fetched successfully", data = orderItem });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching order item: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while fetching the order item");
        }
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetAllUserOrderItems(int userId)
    {
        try
        {
            var orderItems = await _orderItemRepository.GetAllOrderItemsWithUserIdAsync(userId);
            return Ok(new { message = "Order items fetched successfully", data = orderItems });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching order items: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while fetching the order items");
        }
    }

    [HttpDelete("delete/{userId}/{orderItemId}")]
    public async Task<IActionResult> DeleteSpecifiedUserOrderItem(int userId, int orderItemId)
    {
        try
        {
            await _orderItemRepository.DeleteSpecifiedUserOrderItemAsync(userId, orderItemId);
            return Ok(new { message = "Order item deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order item: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while deleting the order item");
        }
    }

    [HttpDelete("delete/{userId}")]
    public async Task<IActionResult> DeleteAllUserOrderItems(int userId)
    {
        try
        {
            await _orderItemRepository.DeleteAllUserOrderItemsAsync(userId);
            return Ok(new { message = "All order items deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting all order items: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while deleting all order items");
        }
    }
}