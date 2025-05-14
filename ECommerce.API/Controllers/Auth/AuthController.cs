using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Auth
{
    [Route("api/v1/auth")]
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

                var result = await _authService.RegisterUserWithRoleAsync(accountRegisterRequestDto, "Admin");
                return Ok(new { message = "Admin user created successfully.", data = result });
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

                var result = await _authService.RegisterUserWithRoleAsync(accountRegisterRequestDto, "User");
                return Ok(new { message = "User created successfully.", data = result });
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

                var loginResult = await _authService.LoginAsync(accountLoginRequestDto);
                return Ok(new { message = "Login successful", data = loginResult });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", accountLoginRequestDto.Email);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {   
            try
            {
                var cookieRefreshToken = await _refreshTokenService.GetRefreshTokenFromCookie();
                if (cookieRefreshToken.IsFailure)
                {
                    return BadRequest(cookieRefreshToken.Error);
                }
                var result = await _refreshTokenService.RevokeUserTokens(cookieRefreshToken.Data.Email, "Logout");
                _refreshTokenService.DeleteRefreshTokenCookie();
                return Ok(new { message = "Logout successful", data = result });
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
                if (cookieRefreshToken.IsFailure)
                {
                    return BadRequest(cookieRefreshToken.Error);
                }
                var authResponse = await _authService.GenerateAuthTokenAsync(cookieRefreshToken.Data);

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
