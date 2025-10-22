using System.Security.Claims;
using ECommerce.Application.Abstract.Service;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Services.Account;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetIpAdress() => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

    public string GetUserEmail()
    {
        string? email = TryGetEmailFromContext();
        return email ?? string.Empty;
    }

    public string GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            return userId;

        return string.Empty;
    }

    private string TryGetEmailFromContext()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            return string.Empty;

        var email = user.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            email = user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

        return email ?? string.Empty;
    }
}