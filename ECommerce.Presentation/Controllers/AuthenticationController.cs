using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthenticationController(IAuthService _authService) : ApiBaseController
    {
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] AccountRegisterRequestDto accountRegisterRequestDto) => HandleResult(await _authService.RegisterAsync(accountRegisterRequestDto, "User"));

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AccountLoginRequestDto accountLoginRequestDto) => HandleResult(await _authService.LoginAsync(accountLoginRequestDto));

        [HttpPost("logout")]
        public async Task<IActionResult> Logout() => HandleResult(await _authService.LogoutAsync());

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRefreshToken() => HandleResult(await _authService.GenerateAuthTokenAsync());
    }
}
