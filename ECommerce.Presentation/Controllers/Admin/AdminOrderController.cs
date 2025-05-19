using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Validations.Attribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/orders")]
[Authorize(Roles = "Admin")]
[ApiVersion("1.0")]
public class AdminOrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public AdminOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(new { message = "Orders fetched successfully", data = orders });
    }

    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetOrderById([FromRoute] int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return Ok(new { message = "Order fetched successfully", data = order });
    }

    [HttpDelete("delete/{id}")]
    [ValidateId]
    public async Task<IActionResult> DeleteOrder([FromRoute] int id)
    {
        var result = await _orderService.DeleteOrderByIdAsync(id);
        return Ok(new { message = "Order deleted successfully with id: " + id, data = result });
    }

    [HttpPut("update/{id}")]
    [ValidateId]
    public async Task<IActionResult> UpdateOrderStatus([FromRoute] int id, [FromBody] OrderUpdateRequestDto orderUpdateRequestDto)
    {
        var result = await _orderService.UpdateOrderStatusByAccountIdAsync(id, orderUpdateRequestDto);
        return Ok(new { message = "Order status updated successfully with id: " + id, data = result });
    }
}