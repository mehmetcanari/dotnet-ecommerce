using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using MediatR;
using ECommerce.Application.Queries.Account;

namespace ECommerce.API.Controllers.Auth;

[ApiController]
[Route("api/v1/account")]
[Authorize(Roles = "User, Admin")]
[ApiVersion("1.0")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public AccountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _mediator.Send(new GetAccountByEmailQuery());
        if (result.IsFailure)
        {
            return BadRequest(new { message = "Failed to fetch profile", error = result.Error });
        }
        return Ok(new { message = "result profile fetched successfully", data = result });
    }
} 