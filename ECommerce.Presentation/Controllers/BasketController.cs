using ECommerce.Application.DTO.Request.BasketItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ECommerce.Application.Queries.Basket;
using ECommerce.Application.Commands.Basket;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize(Roles = "User")]
public class BasketController(IMediator mediator) : ApiBaseController
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateBasketItem([FromBody] CreateBasketItemRequestDto request) => HandleResult(await mediator.Send(new CreateBasketItemCommand(request)));

    [HttpPut("update")]
    public async Task<IActionResult> UpdateBasketItem([FromBody] UpdateBasketItemRequestDto request) => HandleResult(await mediator.Send(new UpdateBasketItemCommand(request)));

    [HttpGet]
    public async Task<IActionResult> GetAllBasketItems() => HandleResult(await mediator.Send(new GetAllBasketItemsQuery()));
}