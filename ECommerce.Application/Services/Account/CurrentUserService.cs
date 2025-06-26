using System.Security.Claims;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Services.Account;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILoggingService _logger;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILoggingService logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Result<string> GetCurrentUserEmail()
    {
        string? email = TryGetEmailFromClaims();

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("User email not found in claims.");
            return Result<string>.Failure("User email not found.");
        }

        _logger.LogInformation("Current user email: {Email}", email);
        return Result<string>.Success(email);
    }

    private string? TryGetEmailFromClaims()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            return null;

        var email = user.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            email = user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

        return email;
    }

    public Result<string> GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Result<string>.Failure("User ID not found.");
        return Result<string>.Success(userId);
    }
    public Result<bool> IsAuthenticated()
    {
        var isAuthenticated = _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        if (!isAuthenticated)
        {
            _logger.LogWarning("User is not authenticated.");
            return Result<bool>.Failure("User is not authenticated.");
        }

        return Result<bool>.Success(isAuthenticated);
    }
}