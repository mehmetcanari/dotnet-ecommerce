using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthenticationController(IAuthService _authService) : ControllerBase
    {
        [HttpPost("create-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAdmin([FromBody] AccountRegisterRequestDto accountRegisterRequestDto)
        {
            var result = await _authService.RegisterAsync(accountRegisterRequestDto, "Admin");
            if (result.IsFailure)
            {
                return BadRequest(new { message = "Failed to create admin user.", error = result.Error });
            }
            return Ok(new { message = "Admin user created successfully." });
        }

        [HttpPost("create-user")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] AccountRegisterRequestDto accountRegisterRequestDto)
        {
            var result = await _authService.RegisterAsync(accountRegisterRequestDto, "User");
            if (result.IsFailure)
            {
                return BadRequest(new { message = "Failed to create user.", error = result.Error });
            }
            return Ok(new { message = "User created successfully." });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AccountLoginRequestDto accountLoginRequestDto)
        {
            var loginResult = await _authService.LoginAsync(accountLoginRequestDto);
            if (loginResult.IsFailure)
            {
                return Unauthorized(new { message = "Invalid credentials", error = loginResult.Error });
            }
            return Ok(new { message = "Login successful", data = loginResult.Data });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _authService.LogoutAsync("User logged out");
            if (result.IsFailure)
            {
                return BadRequest(new { message = "Logout failed", error = result.Error });
            }
            return Ok(new { message = "Logout successful" });
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRefreshToken()
        {
            var authResponse = await _authService.GenerateAuthTokenAsync();
            if (authResponse.IsFailure)
            {
                return BadRequest(new { message = "Failed to refresh token", error = authResponse.Error });
            }
            return Ok(new { message = "Token refreshed successfully", data = authResponse.Data });
        }
    }
}
