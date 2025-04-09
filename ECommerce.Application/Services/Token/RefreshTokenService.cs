using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerce.Application.Services.Token;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository, ILogger<RefreshTokenService> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task CleanupExpiredTokensAsync()
    {
        try
        {
            await _refreshTokenRepository.CleanupExpiredTokensAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired tokens");
        }
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string email, IList<string> roles)
    {
        try
        {
            IEnumerable<RefreshToken> userRefreshTokens = await _refreshTokenRepository.GetUserTokensAsync(email);
            if (userRefreshTokens.Any())
            {
                await _refreshTokenRepository.RevokeAllUserTokensAsync(email, null);
            }

            var refreshTokenExpiry = Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRATION_DAYS");

            var refreshToken = new RefreshToken
            {
                Token = GenerateRefreshJwtToken(email, roles),
                Email = email,
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenExpiry)),
                Created = DateTime.UtcNow
            };

            await _refreshTokenRepository.CreateAsync(refreshToken);
            return refreshToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate refresh token");
            throw new Exception("Failed to generate refresh token", ex);
        }
    }

    public async Task<bool> RevokeAllUserTokensAsync(string email, string? reason = null)
    {
        try
        {
            var tokens = await _refreshTokenRepository.GetUserTokensAsync(email);
            foreach (var token in tokens)
            {
                await _refreshTokenRepository.RevokeAsync(token.Token, reason);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke all user tokens");
            throw new Exception("Failed to revoke all user tokens", ex);
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token, string? reason = null)
    {
        try
        {
            await _refreshTokenRepository.RevokeAsync(token, reason);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke refresh token");
            throw new Exception("Failed to revoke refresh token", ex);
        }
    }

    private string GenerateRefreshJwtToken(string email, IList<string> roles)
    {
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey 
        ?? throw new InvalidOperationException("JWT_SECRET is not configured")));

        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"];
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"];
        var refreshTokenExpiry = Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRATION_DAYS");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("tokenType", "refresh")
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

    public void SetRefreshTokenCookie(RefreshToken refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? _configuration["ASPNETCORE_ENVIRONMENT"],
                "Development",
                StringComparison.OrdinalIgnoreCase),
            SameSite = SameSiteMode.Strict,
            Expires = refreshToken.Expires
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append(
            "refreshToken",
            refreshToken.Token,
            cookieOptions);

        _logger.LogInformation("Refresh token cookie set for user: {Email}", refreshToken.Email);
    }

    public async Task<RefreshToken> GetRefreshTokenFromCookie()
    {
        var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new Exception("Refresh token not found in cookie");
        }
        
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        return token;
    }
}


