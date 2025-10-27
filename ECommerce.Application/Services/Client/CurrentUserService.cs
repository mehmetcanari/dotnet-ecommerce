using System.Security.Claims;
using ECommerce.Application.Abstract;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Services.Client;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string GetIpAddress() => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

    public string GetUserEmail()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
            return string.Empty;

        var email = user.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            email = user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

        return email ?? string.Empty;
    }

    public string GetUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            return userId;

        return string.Empty;
    }
}