using ECommerce.Application.DTO.Request.BasketItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Abstract.Service;
using MediatR;
using ECommerce.Application.Queries.Basket;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize(Roles = "User")]
public class CartItemsController(IBasketItemService _basketItemService, IMediator _mediator) : ApiBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAllBasketItems() => HandleResult(await _mediator.Send(new GetAllBasketItemsQuery()));

    [HttpPost("create")]
    public async Task<IActionResult> CreateBasketItem([FromBody] CreateBasketItemRequestDto createBasketItemRequest) => HandleResult(await _basketItemService.CreateBasketItemAsync(createBasketItemRequest));

    [HttpPut("update")]
    public async Task<IActionResult> UpdateBasketItem([FromBody] UpdateBasketItemRequestDto basketItemRequestUpdateRequest) => HandleResult(await _basketItemService.UpdateBasketItemAsync(basketItemRequestUpdateRequest));

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAllBasketItems() => HandleResult(await _basketItemService.DeleteAllNonOrderedBasketItemsAsync());
}