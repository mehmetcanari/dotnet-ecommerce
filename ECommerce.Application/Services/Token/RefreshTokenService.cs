using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerce.Application.Services.Token;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILoggingService _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository, ILoggingService logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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
            await RevokeUserTokensAsync(email, "User has fresh token");
            var refreshTokenExpiry = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS");

            var refreshToken = new RefreshToken
            {
                Token = GenerateRefreshJwtToken(email, roles),
                Email = email,
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenExpiry)),
                Created = DateTime.UtcNow
            };

            await _refreshTokenRepository.CreateAsync(refreshToken);
            _logger.LogInformation("Refresh token created successfully: {RefreshToken}", refreshToken);
            return refreshToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate refresh token");
            throw;
        }
    }

    public async Task RevokeUserTokensAsync(string email, string reason)
    {
        try
        {
            IEnumerable<RefreshToken> tokens = await _refreshTokenRepository.GetUserTokensAsync(email);
            RefreshToken? activeToken = tokens.FirstOrDefault(t => t.IsExpired == false && t.IsRevoked == false);
            if (activeToken is not null)
            {
                await _refreshTokenRepository.RevokeAsync(activeToken, reason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke user tokens");
            throw;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate refresh token");
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
                SameSite = SameSiteMode.Strict,
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

    public async Task<RefreshToken> GetRefreshTokenFromCookie()
    {
        try
        {
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new Exception("Refresh token not found in cookie");
            }

            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token != null)
            {
                return token;
            }

            throw new Exception("Refresh token not found in database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get refresh token from cookie");
            throw;
        }
    }
}


