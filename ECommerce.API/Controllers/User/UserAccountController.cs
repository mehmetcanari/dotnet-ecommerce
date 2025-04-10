using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Interfaces.Service;

namespace ECommerce.API.Controllers.User;

[ApiController]
[Route("api/account")]
public class UserAccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<UserAccountController> _logger;
    
    public UserAccountController(IAccountService accountService, ILogger<UserAccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }
    
    [Authorize(Roles = "User")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfile([FromBody] AccountUpdateRequestDto accountUpdateRequestDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("User identity claim not found in token");
                return Unauthorized(new { message = "User identity not found" });
            }

            var userEmail = userIdClaim.Value;
            var currentUser = await _accountService.GetAccountByEmailAsync(userEmail);

            await _accountService.UpdateAccountAsync(currentUser.Email, accountUpdateRequestDto);
            return Ok(new { message = "User updated successfully" });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error updating user profile");
            return BadRequest(exception.Message);
        }
    }
} 