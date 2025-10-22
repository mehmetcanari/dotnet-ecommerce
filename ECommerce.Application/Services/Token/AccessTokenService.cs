using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Security.Claims.ClaimTypes;

namespace ECommerce.Application.Services.Token;

public class AccessTokenService : IAccessTokenService
{
    private readonly ILoggingService _logger;

    public AccessTokenService(ILoggingService logger)
    {
        _logger = logger;
    }

    public Result<AccessToken> GenerateAccessTokenAsync(string userId, string email, IList<string> roles)
    {
        try
        {
            var accessTokenExpiry = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION_MINUTES");

            AccessToken accessToken = new AccessToken
            {
                Token = GenerateAccessJwtToken(userId, email, roles),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(accessTokenExpiry))
            };

            return Result<AccessToken>.Success(accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating access token");
            return Result<AccessToken>.Failure(ex.Message);
        }
    }

    private string GenerateAccessJwtToken(string userId, string email, IList<string> roles)
    {
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        var expirationMinutes = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION_MINUTES");

        if (secretKey is not null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var claims = new List<Claim>
            {
                new(NameIdentifier, userId),
                new(Email, email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("tokenType", "access")
            };

            claims.AddRange(roles.Select(role => new Claim(Role, role)));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(expirationMinutes)),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        return string.Empty;
    }
}