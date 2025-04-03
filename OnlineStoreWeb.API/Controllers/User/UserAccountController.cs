using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.Request.Account;
using OnlineStoreWeb.API.Services.Account;
using System.Security.Claims;

namespace OnlineStoreWeb.API.Controllers.User;

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
    public async Task<IActionResult> UpdateProfile([FromBody] AccountUpdateDto accountUpdateDto)
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
                return Forbid("User identity not found");
            }

            var userEmail = userIdClaim.Value;
            var currentUser = await _accountService.GetAccountByEmailAsync(userEmail);
            if (currentUser == null)
            {
                _logger.LogWarning("User account not found for email: {Email}", userEmail);
                return NotFound("User account not found");
            }

            await _accountService.UpdateAccountAsync(currentUser.Email, accountUpdateDto);
            return Ok(new { message = "User updated successfully" });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error updating user profile");
            return BadRequest(exception.Message);
        }
    }
} 