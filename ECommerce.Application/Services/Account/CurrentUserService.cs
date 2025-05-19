using System.Security.Claims;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
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
        var email = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("User email not found in claims.");
            return Result<string>.Failure("User email not found.");
        }

        return Result<string>.Success(email);
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