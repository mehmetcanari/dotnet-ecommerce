using Asp.Versioning;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IRefreshTokenService refreshTokenService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        [HttpPost("create-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAdmin([FromBody] AccountRegisterRequestDto accountRegisterRequestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _authService.RegisterUserWithRoleAsync(accountRegisterRequestDto, "Admin");
                return Ok(new { Message = "Admin user created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin creation");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create-user")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] AccountRegisterRequestDto accountRegisterRequestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _authService.RegisterUserWithRoleAsync(accountRegisterRequestDto, "User");
                return Ok(new { Message = "User created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AccountLoginRequestDto accountLoginRequestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var authResponse = await _authService.LoginAsync(accountLoginRequestDto);
                return Ok(new { message = "Login successful", authResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", accountLoginRequestDto.Email);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {   
            try
            {
                var cookieRefreshToken = await _refreshTokenService.GetRefreshTokenFromCookie();
                await _refreshTokenService.RevokeUserTokens(cookieRefreshToken.Email, "Logout");
                _refreshTokenService.DeleteRefreshTokenCookie();
                return Ok(new { message = "Logout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRefreshToken()
        {
            try
            {
                var cookieRefreshToken = await _refreshTokenService.GetRefreshTokenFromCookie();
                var authResponse = await _authService.GenerateAuthTokenAsync(cookieRefreshToken);

                return Ok(new { message = "Token refreshed successfully", authResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return BadRequest(ex.Message);
            }
        }
    }
}
