using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.API.DTO.Response.Auth;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.API.Services.Token;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenService(IConfiguration configuration, ILogger<TokenService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResponseDto> GenerateAuthTokenAsync(string email, IList<string> roles)
    {
        try
        {
            var accessToken = await GenerateToken(email, roles);
            var refreshTokenString = await GenerateRefreshToken(email);

            var refreshTokenExpiry = DateTime.UtcNow.AddDays(
                Convert.ToDouble(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS")));

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !string.Equals(
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    "Development",
                    StringComparison.OrdinalIgnoreCase),
                SameSite = SameSiteMode.Strict,
                Expires = refreshTokenExpiry
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                "refreshToken",
                refreshTokenString,
                cookieOptions);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"))),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating auth tokens");
            throw;
        }
    }

    public Task<string> GenerateToken(string email, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
                                   throw new InvalidOperationException("JWT Key is not configured")));

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"],
            audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"))),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public Task<string> GenerateRefreshToken(string email)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
                                   throw new InvalidOperationException("JWT Key is not configured")));

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"],
            audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(
                Convert.ToDouble(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS"))),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string? token = null)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                token = _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(token))
                {
                    throw new SecurityTokenException("Refresh token not found in cookie");
                }
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
                                           throw new InvalidOperationException("JWT Key is not configured"))),
                ValidateIssuer = true,
                ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"],
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

            if (!(validatedToken is JwtSecurityToken jwtToken) ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token algorithm");
            }

            return Task.FromResult(principal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating refresh token");
            throw new SecurityTokenException("Invalid refresh token", ex);
        }
    }

    public Task<bool> RevokeRefreshTokenAsync(string? token = null)
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete("refreshToken");
        return Task.FromResult(true);
    }
}