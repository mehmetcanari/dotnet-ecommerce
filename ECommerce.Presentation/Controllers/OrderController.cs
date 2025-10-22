using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Commands.Order;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Queries.Order;
using ECommerce.Application.Validations.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class OrderController(IOrderService _orderService, IMediator _mediator) : ControllerBase
{
    [Authorize("User")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDto orderCreateRequestDto)
    {
        var result = await _orderService.CreateOrderAsync(orderCreateRequestDto);
        if (result.IsFailure)
        {
            return BadRequest(new { message = "Failed to create order", error = result.Error });
        }
        return Ok(new { message = "Order created successfully" });
    }

    [Authorize("User")]
    [HttpGet("client/orders")]
    public async Task<IActionResult> GetClientOrders()
    {
        var userOrders = await _mediator.Send(new GetUserOrdersQuery());
        if (userOrders.IsFailure)
        {
            return BadRequest(new { message = userOrders.Error });
        }
        return Ok(new { message = "Orders fetched successfully", data = userOrders.Data });
    }

    [Authorize("User")]
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelActiveOrder()
    {
        var result = await _mediator.Send(new CancelOrderCommand());
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Order cancelled successfully" });
    }

    [Authorize("Admin")]
    [HttpGet("allOrders")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _mediator.Send(new GetAllOrdersQuery());
        if (orders.IsFailure)
        {
            return NotFound(new { message = orders.Error });
        }
        return Ok(new { message = "Orders fetched successfully", data = orders.Data });
    }

    [Authorize("Admin")]
    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetOrderById([FromRoute] int id)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery { OrderId = id });
        if (order.IsFailure)
        {
            return NotFound(new { message = order.Error });
        }
        return Ok(new { message = "Order fetched successfully", data = order.Data });
    }

    [Authorize("Admin")]
    [HttpDelete("delete/{id}")]
    [ValidateId]
    public async Task<IActionResult> DeleteOrder([FromRoute] int id)
    {
        var result = await _mediator.Send(new DeleteOrderByIdCommand { Id = id });
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Order deleted successfully with id: " + id });
    }

    [Authorize("Admin")]
    [HttpPut("update/{id}")]
    [ValidateId]
    public async Task<IActionResult> UpdateOrderStatus([FromRoute] int id, [FromBody] UpdateOrderStatusRequestDto orderUpdateRequestDto)
    {
        var result = await _orderService.UpdateOrderStatusByAccountIdAsync(id, orderUpdateRequestDto);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Order status updated successfully with id: " + id });
    }
}