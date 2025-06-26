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
        try
        {
            var isAuthenticatedResult = IsAuthenticated();
            if (isAuthenticatedResult.IsFailure || !isAuthenticatedResult.Data)
                return Result<string>.Failure("User is not authenticated.");

            string? email = TryGetEmailFromClaims();

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("User email not found in claims.");
                return Result<string>.Failure("User email not found.");
            }

            _logger.LogInformation("Current user email: {Email}", email);
            return Result<string>.Success(email);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting current user email", exception);
        }

    }

    public Result<string> GetCurrentUserId()
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims.");
                return Result<string>.Failure("User ID not found.");
            }

            _logger.LogInformation("Current user ID: {UserId}", userId);
            return Result<string>.Success(userId);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting current user id", exception);
        }
    }

    public Result<bool> IsAuthenticated()
    {
        try
        {
            var isAuthenticated = _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated)
            {
                _logger.LogWarning("User is not authenticated.");
                return Result<bool>.Failure("User is not authenticated.");
            }
            return Result<bool>.Success(isAuthenticated);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while checking if user is authenticated", exception);
        }
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
}