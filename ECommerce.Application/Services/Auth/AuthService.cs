using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.DTO.Response.Auth;
using ECommerce.Application.Utility;
using ECommerce.Application.Validations.BaseValidator;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Services.Auth;

public class AuthService : BaseValidator, IAuthService
{
    private readonly IAccountService _accountService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenUserClaimsService _tokenUserClaimsService;
    private readonly ILoggingService _logger;
    private readonly ICrossContextUnitOfWork _crossContextUnitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        IAccountService accountService, UserManager<IdentityUser> userManager,
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService,
        ITokenUserClaimsService tokenUserClaimsService,
        ILoggingService logger,
        ICrossContextUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ICurrentUserService currentUserService) : base(serviceProvider)
    {
        _accountService = accountService;
        _userManager = userManager;
        _accessTokenService = accessTokenService;
        _refreshTokenService = refreshTokenService;
        _currentUserService = currentUserService;
        _tokenUserClaimsService = tokenUserClaimsService;
        _logger = logger;
        _crossContextUnitOfWork = unitOfWork;
    }

    public async Task<Result> RegisterAsync(AccountRegisterRequestDto registerRequestDto, string role)
    {
        try
        {
            var validationResult = await ValidateRegistrationRequestAsync(registerRequestDto);
            if (validationResult.IsFailure)
                return validationResult;

            await _crossContextUnitOfWork.BeginTransactionAsync();

            var accountResult = await _accountService.RegisterAccountAsync(registerRequestDto, role);
            if (accountResult.IsFailure && accountResult.Error is not null)
            {
                await _crossContextUnitOfWork.RollbackTransaction();
                return accountResult.IsFailure ? Result.Failure(accountResult.Error) : Result.Success();
            }

            await _crossContextUnitOfWork.CommitTransactionAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.ErrorRegisteringUser, ex.Message);
            await _crossContextUnitOfWork.RollbackTransaction();
            return Result.Failure(ErrorMessages.ErrorRegisteringUser);
        }
    }

    private async Task<Result> ValidateRegistrationRequestAsync(AccountRegisterRequestDto registerRequestDto)
    {
        var validationResult = await ValidateAsync(registerRequestDto);
        if (validationResult is { IsSuccess: false, Error: not null })
            return Result.Failure(validationResult.Error);

        var existingUser = await _userManager.FindByEmailAsync(registerRequestDto.Email);
        if (existingUser != null)
        {
            return Result.Failure(ErrorMessages.AccountEmailAlreadyExists);
        }

        return Result.Success();
    }


    public async Task<Result<AuthResponseDto>> LoginAsync(AccountLoginRequestDto loginRequestDto)
    {
        try
        {
            var validationResult = await ValidateAndReturnAsync(loginRequestDto);
            if (validationResult is { IsSuccess: false, Error: not null })
                return Result<AuthResponseDto>.Failure(validationResult.Error);

            var verifyResult = await VerifyCredentialsAsync(loginRequestDto.Email, loginRequestDto.Password);
            var isValid = verifyResult.Data.Item1;
            var user = verifyResult.Data.Item2;

            if (!isValid || user == null)
            {
                return Result<AuthResponseDto>.Failure(ErrorMessages.InvalidEmailOrPassword);
            }

            var roles = await _userManager.GetRolesAsync(user);

            if(user.Email is null)
                return Result<AuthResponseDto>.Failure(ErrorMessages.IdentityUserNotFound);

            var authResponseDto = await RequestGenerateTokensAsync(user.Id, user.Email, roles);
            if (authResponseDto.IsFailure && authResponseDto.Error is not null)
                return Result<AuthResponseDto>.Failure(authResponseDto.Error);

            if (authResponseDto.Data is null)
                return Result<AuthResponseDto>.Failure(ErrorMessages.UnexpectedAuthenticationError);

            return Result<AuthResponseDto>.Success(authResponseDto.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.ErrorLoggingIn, ex.Message);
            return Result<AuthResponseDto>.Failure(ex.Message);
        }
    }

    public async Task<Result> LogoutAsync()
    {
        try
        {
            var cookieResult = await _refreshTokenService.GetRefreshTokenFromCookie();
            if (cookieResult is { IsFailure: true, Error: not null })
                return Result.Failure(cookieResult.Error);

            var refreshToken = cookieResult.Data;
            if (refreshToken != null)
            {
                var request = new TokenRevokeRequestDto { Email = refreshToken.Email, Reason = string.Empty };
                await _refreshTokenService.RevokeUserTokens(request);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.FailedToRevokeToken, ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<AuthResponseDto>> GenerateAuthTokenAsync()
    {
        try
        {
            var cookieRefreshToken = await _refreshTokenService.GetRefreshTokenFromCookie();
            if (cookieRefreshToken.Data is null)
            {
                return Result<AuthResponseDto>.Failure(ErrorMessages.UserIsNotLoggedIn);
            }

            var identifier = _tokenUserClaimsService.GetClaimsPrincipalFromToken(cookieRefreshToken.Data);
            var validateResult = await _refreshTokenService.ValidateRefreshToken(identifier, _userManager);
            var email = validateResult.Data.Item1;
            var roles = validateResult.Data.Item2;

            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user is null)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.IdentityUserNotFound);
            }

            var authResponseDto = await RequestGenerateTokensAsync(userId, email, roles);
            if(authResponseDto.IsFailure && authResponseDto.Error is not null)
                return Result<AuthResponseDto>.Failure(authResponseDto.Error);

            if(authResponseDto.Data is null)
                return Result<AuthResponseDto>.Failure(ErrorMessages.UnexpectedAuthenticationError);

            return Result<AuthResponseDto>.Success(authResponseDto.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedAuthenticationError);
            return Result<AuthResponseDto>.Failure(ex.Message);
        }
    }

    private async Task<Result<AuthResponseDto>> RequestGenerateTokensAsync(string userId, string email, IList<string> roles)
    {
        try
        {
            var accessToken = _accessTokenService.GenerateAccessTokenAsync(userId, email, roles);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(userId, email, roles);

            if (refreshToken.Data is null)
                return Result<AuthResponseDto>.Failure(ErrorMessages.FailedToGenerateRefreshToken);

            if (accessToken.Data is null)
                return Result<AuthResponseDto>.Failure(ErrorMessages.FailedToGenerateAccessToken);

            _refreshTokenService.SetRefreshTokenCookie(refreshToken.Data);

            var authResponseDto = new AuthResponseDto
            {
                AccessToken = accessToken.Data.Token,
                AccessTokenExpiration = accessToken.Data.Expires,
            };

            return Result<AuthResponseDto>.Success(authResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.ErrorGeneratingTokens);
            return Result<AuthResponseDto>.Failure(ex.Message);
        }
    }

    private async Task<Result<(bool, IdentityUser?)>> VerifyCredentialsAsync(string email, string password)
    {
        try
        {
            var account = await _accountService.GetAccountByEmailAsEntityAsync(email);
            if (account.IsFailure || account.Data == null)
                return Result<(bool, IdentityUser?)>.Failure(ErrorMessages.AccountNotFound);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<(bool, IdentityUser?)>.Failure(ErrorMessages.IdentityUserNotFound);

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                return Result<(bool, IdentityUser?)>.Failure(ErrorMessages.InvalidEmailOrPassword);

            if (account.Data is { IsBanned: true })
                return Result<(bool, IdentityUser?)>.Failure(ErrorMessages.AccountBanned);

            return Result<(bool, IdentityUser?)>.Success((true, user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.ErrorValidatingLogin);
            return Result<(bool, IdentityUser?)>.Failure(ErrorMessages.ErrorValidatingLogin);
        }
    }
}