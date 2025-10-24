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
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Validations.BaseValidator;
using ECommerce.Shared.Constants;
namespace ECommerce.Application.Services.Token;

public class RefreshTokenService : BaseValidator, IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILoggingService _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    public const string Reason = "New refresh token generated";

    public RefreshTokenService(
        IRefreshTokenRepository refreshTokenRepository, 
        ILoggingService logger, 
        IConfiguration configuration, 
        IHttpContextAccessor httpContextAccessor, 
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RefreshToken>> GenerateRefreshTokenAsync(Guid userId, string email, IList<string> roles)
    {
        try
        {
            var request = new TokenRevokeRequestDto { Email = email, Reason = Reason };
            await RevokeUserTokens(request);
            var refreshTokenExpiry = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS");

            var refreshToken = new RefreshToken
            {
                Token = GenerateRefreshJwtToken(userId, email, roles),
                Email = email,
                ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenExpiry))
            };

            await _refreshTokenRepository.CreateAsync(refreshToken);
            await _unitOfWork.Commit();
            return Result<RefreshToken>.Success(refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.FailedToGenerateRefreshToken);
            throw;
        }
    }

    public async Task<Result<(string, IList<string>)>> ValidateRefreshToken(ClaimsPrincipal principal, UserManager<User> userManager)
    {
        try
        {
            var email = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(email))
                return Result<(string, IList<string>)>.Failure(ErrorMessages.InvalidToken);

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<(string, IList<string>)>.Failure(ErrorMessages.IdentityUserNotFound);

            var roles = await userManager.GetRolesAsync(user);

            return Result<(string, IList<string>)>.Success((user.Email ?? string.Empty, roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.ErrorValidatingToken);
            throw;
        }
    }

    public async Task<Result> RevokeUserTokens(TokenRevokeRequestDto request)
    {
        try
        {
            var validationResult = await ValidateAsync(request);
            if (validationResult is { IsSuccess: false, Message: not null })
                return Result.Failure(validationResult.Message);

            var token = await _refreshTokenRepository.GetActiveUserTokenAsync(request.Email);
            if (token == null)
                return Result.Failure(ErrorMessages.NoActiveTokensFound);

            _httpContextAccessor.HttpContext?.Response.Cookies.Delete("refreshToken");
            _refreshTokenRepository.Revoke(token, request.Reason);
            await _unitOfWork.Commit();
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.FailedToRevokeToken);
            return Result.Failure(ex.Message);
        }
    }

    private string GenerateRefreshJwtToken(Guid userId, string email, IList<string> roles)
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
            _logger.LogError(ex, ErrorMessages.FailedToGenerateRefreshToken);
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
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development",
                    StringComparison.OrdinalIgnoreCase),
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenExpiry))
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            throw;
        }
    }
    
    public async Task<Result<RefreshToken>> GetRefreshTokenFromCookie()
    {
        try
        {
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Result<RefreshToken>.Failure(ErrorMessages.FailedToFetchUserTokens);

            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null)
                return Result<RefreshToken>.Failure(ErrorMessages.FailedToFetchUserTokens);
            
            return Result<RefreshToken>.Success(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.FailedToFetchUserTokens, ex.Message);
            return Result<RefreshToken>.Failure(ex.Message);
        }
    }
}


