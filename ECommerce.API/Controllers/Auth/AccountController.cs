using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ECommerce.Application.Interfaces.Service;
using Asp.Versioning;
using ECommerce.Domain.Abstract.Repository;

namespace ECommerce.API.Controllers.Auth;

[ApiController]
[Route("api/v1/account")]
[Authorize(Roles = "User, Admin")]
[ApiVersion("1.0")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILoggingService _logger;
    
    public AccountController(IAccountService accountService, ILoggingService logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Email);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User identity not found" });
            }

            var userEmail = userIdClaim.Value;

            var user = await _accountService.GetAccountByEmailAsync(userEmail);
            return Ok(new { message = "User profile fetched successfully", user });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return BadRequest(ex.Message);
        }
    }
} 