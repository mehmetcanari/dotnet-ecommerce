using ECommerce.Application.Utility;
using ECommerce.Domain.Model;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Shared.Constants;
using static System.Security.Claims.ClaimTypes;
using ECommerce.Application.Abstract;

namespace ECommerce.Application.Commands.Token
{
    public class CreateAccessTokenCommand(Guid userId, string email, IList<string> roles) : IRequest<Result<AccessToken>>
    {
        public readonly Guid UserId = userId;
        public readonly string Email = email;
        public readonly IList<string> Roles = roles;
    }

    public class CreateAccessTokenCommandHandler(ILogService logService) : IRequestHandler<CreateAccessTokenCommand, Result<AccessToken>>
    {
        public async Task<Result<AccessToken>> Handle(CreateAccessTokenCommand request, CancellationToken cancellationToken)
        {
            var accessToken = GenerateAccessTokenAsync(request.UserId, request.Email, request.Roles);
            return accessToken;
        }

        private Result<AccessToken> GenerateAccessTokenAsync(Guid userId, string email, IList<string> roles)
        {
            try
            {
                var accessTokenExpiry = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION_MINUTES");

                var accessToken = new AccessToken
                {
                    Token = GenerateAccessJwtToken(userId, email, roles),
                    Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(accessTokenExpiry))
                };

                return Result<AccessToken>.Success(accessToken);
            }
            catch (Exception ex)
            {
                logService.LogError(ex, ErrorMessages.ErrorGeneratingTokens);
                return Result<AccessToken>.Failure(ex.Message);
            }
        }

        private string GenerateAccessJwtToken(Guid userId, string email, IList<string> roles)
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            var expirationMinutes = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION_MINUTES");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? string.Empty));

            var claims = new List<Claim>
            {
                new(NameIdentifier, userId.ToString()),
                new(Email, email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("tokenType", "access")
            };

            claims.AddRange(roles.Select(role => new Claim(Role, role)));

            var token = new JwtSecurityToken(issuer: issuer, audience: audience, claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(expirationMinutes)),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
