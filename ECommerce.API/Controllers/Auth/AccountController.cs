using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using ECommerce.Domain.Abstract.Repository;

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
    public async Task<IActionResult> GetProfile()
    {
        /*var userIdClaim = User.FindFirst(ClaimTypes.Email); //TODO: User claim logic must be in service layer
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User identity not found" });
            }

            var userEmail = userIdClaim.Value;*/

        var user = await _accountService.GetAccountByEmailAsResponseAsync(userEmail);
        return Ok(new { message = "User profile fetched successfully", data = user });
    }
} 