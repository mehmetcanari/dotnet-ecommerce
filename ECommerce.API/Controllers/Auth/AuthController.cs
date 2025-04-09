using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
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

                await _authService.RegisterAdminAsync(accountRegisterRequestDto);
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

                await _authService.RegisterUserAsync(accountRegisterRequestDto);
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
                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", accountLoginRequestDto.Email);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRefreshToken()
        {
            try
            {
                var refreshToken = await _refreshTokenService.GetRefreshTokenFromCookie();
                var authResponse = await _authService.GenerateAuthTokenAsync(refreshToken);

                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return BadRequest(ex.Message);
            }
        }
    }
}
