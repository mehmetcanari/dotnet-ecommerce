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

        public AuthController(IAuthService authService, IRefreshTokenService refreshTokenService)
        {
            _authService = authService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost("create-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAdmin([FromBody] AccountRegisterRequestDto accountRegisterRequestDto)
        {
            var result = await _authService.RegisterUserWithRoleAsync(accountRegisterRequestDto, "Admin");
            return Ok(new { message = "Admin user created successfully.", data = result });
        }

        [HttpPost("create-user")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] AccountRegisterRequestDto accountRegisterRequestDto)
        {
            var result = await _authService.RegisterUserWithRoleAsync(accountRegisterRequestDto, "User");
            return Ok(new { message = "User created successfully.", data = result });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AccountLoginRequestDto accountLoginRequestDto)
        {
            var loginResult = await _authService.LoginAsync(accountLoginRequestDto);
            return Ok(new { message = "Login successful", data = loginResult });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {   
            
            //TODO: Refactor here, this is not a good practice
            var cookieRefreshToken = await _refreshTokenService.GetRefreshTokenFromCookie();
            if (cookieRefreshToken.IsFailure)
            {
                return BadRequest(cookieRefreshToken.Error);
            }
            var result = await _refreshTokenService.RevokeUserTokens(cookieRefreshToken.Data.Email, "Logout");
            _refreshTokenService.DeleteRefreshTokenCookie();
            return Ok(new { message = "Logout successful", data = result });
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRefreshToken()
        {
            //TODO: Refactor here, this is not a good practice
            var cookieRefreshToken = await _refreshTokenService.GetRefreshTokenFromCookie();
            if (cookieRefreshToken.IsFailure)
            {
                return BadRequest(cookieRefreshToken.Error);
            }
            var authResponse = await _authService.GenerateAuthTokenAsync(cookieRefreshToken.Data);

            return Ok(new { message = "Token refreshed successfully", authResponse });
        }
    }
}
