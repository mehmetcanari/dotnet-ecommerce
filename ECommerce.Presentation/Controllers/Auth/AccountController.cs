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
    private readonly IAccountService _accountService;
    private readonly IMediator _mediator;
    
    public AccountController(IAccountService accountService, IMediator mediator)
    {
        _accountService = accountService;
        _mediator = mediator;
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _mediator.Send(new GetAccountByEmailQuery());
        if (user is { IsFailure: true, Error: not null })
        {
            return BadRequest(new { message = user.Error });
        }
        return Ok(new { message = "User profile fetched successfully", data = user });
    }
} 