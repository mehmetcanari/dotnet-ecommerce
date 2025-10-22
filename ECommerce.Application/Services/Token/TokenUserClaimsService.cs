using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Application.Abstract.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;

namespace ECommerce.Application.Services.Token;

public class TokenUserClaimsService : ITokenUserClaimsService
{
    private readonly IConfiguration _configuration;
    private readonly ILoggingService _logger;

    public TokenUserClaimsService(IConfiguration configuration, ILoggingService logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public ClaimsPrincipal GetClaimsPrincipalFromToken(RefreshToken refreshToken)
    {
        try
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey 
                                                                      ?? throw new InvalidOperationException("JWT_SECRET is not configured")));
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"];
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"];

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(refreshToken.Token, tokenValidationParameters, out var validatedToken);

            if (!(validatedToken is JwtSecurityToken jwtToken) ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException(ErrorMessages.InvalidTokenAlgorithm);
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.ErrorValidatingToken, ex.Message);
            throw new SecurityTokenException(ErrorMessages.ErrorValidatingToken, ex);
        }
    }
}