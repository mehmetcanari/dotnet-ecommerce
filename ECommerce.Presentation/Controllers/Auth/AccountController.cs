using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ECommerce.Application.Abstract.Service;

namespace ECommerce.API.Controllers.Auth;

[ApiController]
[Route("api/v1/account")]
[Authorize(Roles = "User, Admin")]
[ApiVersion("1.0")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    
    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _accountService.GetAccountByEmailAsResponseAsync();
        return Ok(new { message = "User profile fetched successfully", data = user });
    }
} 