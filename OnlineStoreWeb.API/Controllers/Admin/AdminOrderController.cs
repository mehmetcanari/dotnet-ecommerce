using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.Order;
using OnlineStoreWeb.API.Model;
using OnlineStoreWeb.API.Services.Order;

namespace OnlineStoreWeb.API.Controllers.Admin;

[ApiController]
[Route("api/admin/orders")]
public class AdminOrderController(
    IOrderService orderService,
    IValidator<OrderUpdateDto> orderUpdateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        try
        {
            var orders = await orderService.GetAllOrdersAsync();
            return Ok(new { message = "Orders fetched successfully", data = orders });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while fetching orders");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        try
        {
            var order = await orderService.GetOrderWithIdAsync(id);
            return Ok(new { message = "Order fetched successfully", data = order });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while fetching the order");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        try
        {
            await orderService.DeleteOrderAsync(id);
            return Ok(new { message = "Order deleted successfully with id: " + id });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while deleting the order");
        }
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateOrderStatus(int id, OrderUpdateDto orderUpdateDto)
    {
        ValidationResult validationResult = await orderUpdateValidator.ValidateAsync(orderUpdateDto);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(this.ModelState, null);
            return BadRequest(this.ModelState);
        }
        
        try
        {
            await orderService.UpdateOrderStatusAsync(id, orderUpdateDto);
            return Ok(new { message = "Order status updated successfully with id: " + id });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while updating the order status");
        }
    }
}