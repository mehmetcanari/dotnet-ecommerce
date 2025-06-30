using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Commands.Order;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Queries.Order;
using ECommerce.Application.Validations.Attribute;
using MediatR;
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
    private readonly IMediator _mediator;

    public AdminOrderController(IOrderService orderService, IMediator mediator)
    {
        _orderService = orderService;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _mediator.Send(new GetAllOrdersQuery());
        if (orders.IsFailure)
        {
            return NotFound(new { message = orders.Error });
        }
        return Ok(new { message = "Orders fetched successfully", data = orders.Data });
    }

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

    [HttpPut("update/{id}")]
    [ValidateId]
    public async Task<IActionResult> UpdateOrderStatus([FromRoute] int id, [FromBody] OrderUpdateRequestDto orderUpdateRequestDto)
    {
        var result = await _orderService.UpdateOrderStatusByAccountIdAsync(id, orderUpdateRequestDto);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Order status updated successfully with id: " + id });
    }
}