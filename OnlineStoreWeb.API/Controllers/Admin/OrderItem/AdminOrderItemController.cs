using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin/orderitems")]
public class AdminOrderItemController : ControllerBase
{
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ILogger<AdminOrderItemController> _logger;

    public AdminOrderItemController(IOrderItemRepository orderItemRepository, ILogger<AdminOrderItemController> logger)
    {
        _orderItemRepository = orderItemRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrderItems()
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemRepository.Get();
            return Ok(new { message = "Order items fetched successfully", data = orderItems });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching order items: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while fetching the order items");
        }
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetAllOrderItemsWithUserId(int userId)
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemRepository.GetAllOrderItemsWithUserIdAsync(userId);
            return Ok(new { message = "Order items fetched successfully", data = orderItems });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching order item: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while fetching the order item");
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAllOrderItems()
    {
        try
        {
            await _orderItemRepository.DeleteAllOrderItemsAsync();
            return Ok(new { message = "All order items deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order items: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while deleting the order items");
        }
    }

}