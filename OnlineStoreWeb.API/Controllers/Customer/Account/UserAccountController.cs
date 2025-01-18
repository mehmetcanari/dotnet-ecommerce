using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.User;

[ApiController]
[Route("api/user")]
public class UserAccountController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<UserAccountController> _logger;

    public UserAccountController(IAccountRepository accountRepository, ILogger<UserAccountController> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(AccountRegisterDto accountRegisterRequest)
    {
        try
        {
            if (accountRegisterRequest == null)
                return BadRequest(new { message = "User data is required" });

            await _accountRepository.AddAccountAsync(accountRegisterRequest);
            return Created($"users", new { message = "User created successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating user: {Message}", ex.Message);
            return BadRequest(new { message = "Invalid user data provided" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating user: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while creating the user");
        }
    }

    [HttpPut("profile/{id}")]
    public async Task<IActionResult> UpdateProfile(int id, AccountUpdateDto accountUpdateRequest)
    {
        try
        {
            if (accountUpdateRequest == null)
                return BadRequest(new { message = "User update data is required" });

            await _accountRepository.UpdateAccountAsync(id, accountUpdateRequest);
            return Ok(new { message = "User updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating user: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while updating the user");
        }
    }
} 