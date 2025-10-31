using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerce.Application.Commands.Token
{
    public class CreateRefreshTokenCommand(Guid userId, string email, IList<string> roles) : IRequest<Result<RefreshToken>>
    {
        public readonly Guid UserId = userId;
        public readonly string Email = email;
        public readonly IList<string> Roles = roles;
    }

    public class CreateRefreshTokenCommandHandler(ILogService logService, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, IMediator mediator, IHttpContextAccessor contextAccessor) 
        : IRequestHandler<CreateRefreshTokenCommand, Result<RefreshToken>>
    {
        private const string Reason = "Generating new refresh token";

        public async Task<Result<RefreshToken>> Handle(CreateRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var tokenResult = await GenerateRefreshTokenAsync(request.UserId, request.Email, request.Roles, cancellationToken);
            if (tokenResult is { IsFailure: true })
                return Result<RefreshToken>.Failure(tokenResult.Message ?? ErrorMessages.FailedToGenerateRefreshToken);

            var refreshToken = tokenResult.Data;
            if(refreshToken is null)
                return Result<RefreshToken>.Failure(ErrorMessages.FailedToGenerateRefreshToken);

            return Result<RefreshToken>.Success(refreshToken);
        }

        private async Task<Result<RefreshToken>> GenerateRefreshTokenAsync(Guid userId, string email, IList<string> roles, CancellationToken cancellationToken) 
        {
            try
            {
                var request = new TokenRevokeRequestDto { Email = email, Reason = Reason};
                await mediator.Send(new RevokeRefreshTokenCommand(request), cancellationToken);

                var refreshTokenExpiry = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS");
                var refreshToken = new RefreshToken
                {
                    Token = GenerateToken(userId, email, roles),
                    Email = email,
                    ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenExpiry))
                };

                await refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);
                AddRefreshTokenInContext(refreshToken);
                await unitOfWork.Commit();
                return Result<RefreshToken>.Success(refreshToken);
            }
            catch (Exception ex)
            {
                logService.LogError(ex, ErrorMessages.FailedToGenerateRefreshToken);
                throw;
            }
        }

        private string GenerateToken(Guid userId, string email, IList<string> roles)
        {
            try
            {
                var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? throw new InvalidOperationException("JWT_SECRET is not configured")));
                var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
                var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
                var refreshTokenExpiry = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS");
                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, email),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new("tokenType", "refresh"),
                    new(ClaimTypes.NameIdentifier, userId.ToString())
                };
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var token = new JwtSecurityToken(issuer: issuer, audience: audience, claims: claims,
                    expires: DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenExpiry)),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                logService.LogError(ex, ErrorMessages.FailedToGenerateRefreshToken);
                throw;
            }
        }

        private void AddRefreshTokenInContext(RefreshToken refreshToken)
        {
            try
            {
                var refreshTokenExpiry = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS");
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !string.Equals(
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development",
                        StringComparison.OrdinalIgnoreCase),
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenExpiry))
                };

                contextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
            }
            catch (Exception ex)
            {
                logService.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
                throw;
            }
        }
    }
}
