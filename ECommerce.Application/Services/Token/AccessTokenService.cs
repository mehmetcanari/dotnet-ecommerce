using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Application.Abstract.Service;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using static System.Security.Claims.ClaimTypes;

namespace ECommerce.Application.Services.Token;

public class AccessTokenService : IAccessTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILoggingService _logger;

    public AccessTokenService(IConfiguration configuration, ILoggingService logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AccessToken> GenerateAccessTokenAsync(string email, IList<string> roles)
    {
        try
        {
            var accessTokenExpiry = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION_MINUTES");

            AccessToken accessToken = new AccessToken
            {
                Token = GenerateAccessJwtToken(email, roles),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(accessTokenExpiry))
            };

            return await Task.FromResult(accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating access token");
            throw;
        }
    }

    private string GenerateAccessJwtToken(string email, IList<string> roles)
    {
        try
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey
            ?? throw new InvalidOperationException("JWT_SECRET is not configured")));

            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"];
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"];
            var expirationMinutes = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION_MINUTES");

            var claims = new List<Claim>
            {
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating access token");
            throw;
        }
    }
}