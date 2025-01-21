using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.OrderItem;
using OnlineStoreWeb.API.Services.OrderItem;

namespace OnlineStoreWeb.API.Controllers.Admin.OrderItem;

[ApiController]
[Route("api/admin/orderitems")]
public class AdminOrderItemController(IOrderItemService orderItemService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllOrderItems()
    {
        try
        {
            List<Model.OrderItem> orderItems = await orderItemService.GetAllOrderItemsAsync();
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
            List<Model.OrderItem> orderItems = await orderItemService.GetAllOrderItemsWithUserIdAsync(userId);
            return Ok(new { message = "Order items fetched successfully", data = orderItems });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while fetching the order item");
        }
    }
}