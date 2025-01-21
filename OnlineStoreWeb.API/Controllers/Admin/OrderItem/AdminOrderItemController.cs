using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin/orderitems")]
public class AdminOrderItemController : ControllerBase
{
    private readonly IOrderItemService _orderItemService;

    public AdminOrderItemController(IOrderItemService orderItemService)
    {
        _orderItemService = orderItemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrderItems()
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemService.GetAllOrderItemsAsync();
            return Ok(new { message = "Order items fetched successfully", data = orderItems });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while fetching the order items");
        }
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetAllOrderItemsWithUserId(int userId)
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemService.GetAllOrderItemsWithUserIdAsync(userId);
            return Ok(new { message = "Order items fetched successfully", data = orderItems });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while fetching the order item");
        }
    }
}