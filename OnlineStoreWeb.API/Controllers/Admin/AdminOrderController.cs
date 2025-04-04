using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.Request.Order;
using OnlineStoreWeb.API.Services.Order;

namespace OnlineStoreWeb.API.Controllers.Admin;

[ApiController]
[Route("api/admin/orders")]
public class AdminOrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public AdminOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(new { message = "Orders fetched successfully", data = orders });
        }
        catch (Exception exception)
        {
            return BadRequest( exception.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById([FromRoute] int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(new { message = "Order fetched successfully", data = order });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteOrder([FromRoute] int id)
    {
        try
        {
            await _orderService.DeleteOrderByIdAsync(id);
            return Ok(new { message = "Order deleted successfully with id: " + id });
        }
        catch (Exception exception) 
        {
            return BadRequest(exception.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateOrderStatus([FromRoute] int id, [FromBody] OrderUpdateRequestDto orderUpdateRequestDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            await _orderService.UpdateOrderStatusByAccountIdAsync(id, orderUpdateRequestDto);
            return Ok(new { message = "Order status updated successfully with id: " + id });
        }
        catch (Exception exception)
        {
            return BadRequest( exception.Message);
        }
    }
}