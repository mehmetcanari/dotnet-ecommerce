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
public class OrderController(IOrderService _orderService, IMediator _mediator) : ApiBaseController
{
    [Authorize("User")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDto orderCreateRequestDto) => HandleResult(await _orderService.CreateOrderAsync(orderCreateRequestDto));

    [Authorize("User")]
    [HttpGet("client/orders")]
    public async Task<IActionResult> GetUserOrders() => HandleResult(await _mediator.Send(new GetUserOrdersQuery()));

    [Authorize("User")]
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelActiveOrder() => HandleResult(await _mediator.Send(new CancelOrderCommand()));

    [Authorize("Admin")]
    [HttpGet("allOrders")]
    public async Task<IActionResult> GetAllOrders() => HandleResult(await _mediator.Send(new GetAllOrdersQuery()));

    [Authorize("Admin")]
    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetOrderById([FromRoute] Guid id) => HandleResult(await _mediator.Send(new GetOrderByIdQuery { Id = id }));

    [Authorize("Admin")]
    [HttpDelete("delete/{id}")]
    [ValidateId]
    public async Task<IActionResult> DeleteOrder([FromRoute] Guid id) => HandleResult(await _mediator.Send(new DeleteOrderByIdCommand { Id = id }));

    [Authorize("Admin")]
    [HttpPut("update/{id}")]
    [ValidateId]
    public async Task<IActionResult> UpdateOrderStatus([FromRoute] Guid id, [FromBody] UpdateOrderStatusRequestDto orderUpdateRequestDto) => HandleResult(await _orderService.UpdateOrderStatus(id, orderUpdateRequestDto));
}