using ECommerce.Application.Commands.Auth;
using ECommerce.Shared.DTO.Request.Account;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthenticationController(IMediator mediator) : ApiBaseController
    {
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] AccountRegisterRequestDto request) => HandleResult(await mediator.Send(new RegisterCommand(request)));

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AccountLoginRequestDto request) => HandleResult(await mediator.Send(new LoginCommand(request)));

        [HttpPost("logout")]
        public async Task<IActionResult> Logout() => HandleResult(await mediator.Send(new LogoutCommand()));
    }
}
