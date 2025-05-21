using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;

namespace ECommerce.Application.Services.Token;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILoggingService _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenService(
        IRefreshTokenRepository refreshTokenRepository, 
        ILoggingService logger, 
        IConfiguration configuration, 
        IHttpContextAccessor httpContextAccessor, 
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<RefreshToken>> GenerateRefreshTokenAsync(string email, IList<string> roles)
    {
        try
        {
            await RevokeUserTokens(email, "New refresh token generated");
            var refreshTokenExpiry = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS");

            var refreshToken = new RefreshToken
            {
                Token = GenerateRefreshJwtToken(email, roles),
                Email = email,
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenExpiry)),
                Created = DateTime.UtcNow
            };

            await _refreshTokenRepository.CreateAsync(refreshToken);
            await _unitOfWork.Commit();
            _logger.LogInformation("Refresh token created successfully: {RefreshToken}", refreshToken);
            return Result<RefreshToken>.Success(refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate refresh token");
            throw;
        }
    }

    public async Task<(string, IList<string>)> ValidateRefreshToken(ClaimsPrincipal principal, UserManager<IdentityUser> userManager)
    {
        try
        {
            var email = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                throw new Exception("Email claim not found in token");
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var roles = await userManager.GetRolesAsync(user);

            return (email, roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating refresh token");
            throw;
        }
    }

    public async Task<Result> RevokeUserTokens(string email, string reason)
    {
        try
        {
            var token = await _refreshTokenRepository.GetActiveUserTokenAsync(email);
            if (token == null)
            {
                _logger.LogWarning("No active refresh token found for user: {Email}", email);
                return Result.Failure("No active refresh token found");
            }

            DeleteRefreshTokenCookie();
            _refreshTokenRepository.Revoke(token, reason);
            await _unitOfWork.Commit();
            
            _logger.LogInformation("User tokens revoked successfully: {Email}", email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke user tokens");
            return Result.Failure(ex.Message);
        }
    }

    private string GenerateRefreshJwtToken(string email, IList<string> roles)
    {
        try
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? throw new InvalidOperationException("JWT_SECRET is not configured")));
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"];
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"];
            var refreshTokenExpiry = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS");
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("tokenType", "refresh")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                 issuer: issuer,
                 audience: audience,
                 claims: claims,
                 expires: DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenExpiry)),
                 signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
             );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate refresh token");
            throw;
        }
    }

    private void DeleteRefreshTokenCookie()
    {
        try
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete("refreshToken");
            _logger.LogInformation("Refresh token cookie deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete refresh token cookie");
            throw;
        }
    }

    public void SetRefreshTokenCookie(RefreshToken refreshToken)
    {
        try
        {
            var refreshTokenExpiry = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS");
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !string.Equals(
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? _configuration["ASPNETCORE_ENVIRONMENT"],
                    "Development",
                    StringComparison.OrdinalIgnoreCase),
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenExpiry))
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                "refreshToken",
                refreshToken.Token,
                cookieOptions);

            _logger.LogInformation("Refresh token cookie set for user: {Email}", refreshToken.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set refresh token cookie");
            throw;
        }
    }
    
    public async Task<Result<RefreshToken>> GetRefreshTokenFromCookie()
    {
        try
        {
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Result<RefreshToken>.Failure("User is not logged in");
            }

            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null)
            {
                return Result<RefreshToken>.Failure("Refresh token not found");
            }
            
            return Result<RefreshToken>.Success(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get refresh token from cookie");
            return Result<RefreshToken>.Failure(ex.Message);
        }
    }
}


