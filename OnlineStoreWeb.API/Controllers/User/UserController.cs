using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OnlineStoreWeb.API.Controllers.User;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserRepository userRepository, ILogger<UserController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto userDto)
    {
        try
        {
            if (userDto == null)
                return BadRequest(new { message = "User data is required" });

            await _userRepository.AddUserAsync(userDto);
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

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userRepository.GetAllUsersAsync();
            if (!users.Any())
                return NotFound(new { message = "No users found" });

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching users: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while fetching users");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserWithId(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid user ID" });

            var user = await _userRepository.GetUserWithIdAsync(id);
            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User fetching process error: {Message}", ex.Message);
            return BadRequest(new { message = "User fetching process error" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching user: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while fetching the user");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateDto)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid user ID" });

            if (updateDto == null)
                return BadRequest(new { message = "User update data is required" });

            await _userRepository.UpdateUserAsync(id, updateDto);
            return Ok(new { message = "User updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error while updating user: {Message}", ex.Message);
            return BadRequest(new { message = "Invalid user data provided" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found for update: {Message}", ex.Message);
            return NotFound(new { message = $"User with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating user: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while updating the user");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid user ID" });

            await _userRepository.DeleteUserAsync(id);
            return Ok(new { message = "User deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found for deletion: {Message}", ex.Message);
            return NotFound(new { message = $"User with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting user: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while deleting the user");
        }
    }
} 