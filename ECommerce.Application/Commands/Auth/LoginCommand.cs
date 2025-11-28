using ECommerce.Application.Abstract;
using ECommerce.Application.Commands.Token;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Auth;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Commands.Auth
{
    public class LoginCommand(AccountLoginRequestDto request) : IRequest<Result<AuthResponseDto>>
    {
        public readonly AccountLoginRequestDto Model = request;
    }

    public class LoginCommandHandler(IUserRepository userRepository, UserManager<User> userManager, ILogService logService, IMediator mediator) : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var verifyResult = await VerifyCredentialsAsync(request.Model.Email, request.Model.Password);
                if (verifyResult is { IsFailure: true })
                    return Result<AuthResponseDto>.Failure(ErrorMessages.InvalidCredentials);

                var user = verifyResult.Data;
                if (user is null)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.IdentityUserNotFound);

                var roles = await userManager.GetRolesAsync(user);
                var email = user.Email;
                if (email is null)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.AccountEmailNotFound);

                var authResponseDto = await RequestGenerateTokensAsync(user.Id, email, roles);
                if (authResponseDto is { IsFailure: true, Message: not null })
                    return Result<AuthResponseDto>.Failure(authResponseDto.Message);

                if (authResponseDto.Data is null)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.UnexpectedAuthenticationError);

                return Result<AuthResponseDto>.Success(authResponseDto.Data);
            }
            catch (Exception ex)
            {
                logService.LogError(ex, ErrorMessages.ErrorLoggingIn, ex.Message);
                return Result<AuthResponseDto>.Failure(ex.Message);
            }
        }

        private async Task<Result<User>> VerifyCredentialsAsync(string email, string password)
        {
            try
            {
                var account = await userRepository.GetByEmail(email);
                if (account is null)
                    return Result<User>.Failure(ErrorMessages.AccountNotFound);

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                    return Result<User>.Failure(ErrorMessages.IdentityUserNotFound);

                var isPasswordValid = await userManager.CheckPasswordAsync(user, password);
                if (!isPasswordValid)
                    return Result<User>.Failure(ErrorMessages.InvalidEmailOrPassword);

                if (account is { IsBanned: true })
                    return Result<User>.Failure(ErrorMessages.AccountBanned);

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                logService.LogError(ex, ErrorMessages.ErrorValidatingLogin);
                return Result<User>.Failure(ErrorMessages.ErrorValidatingLogin);
            }
        }

        private async Task<Result<AuthResponseDto>> RequestGenerateTokensAsync(Guid userId, string email, IList<string> roles)
        {
            try
            {
                var accessTokenResult = await mediator.Send(new CreateAccessTokenCommand(userId, email, roles));
                if (accessTokenResult is { IsFailure: true })
                    return Result<AuthResponseDto>.Failure(ErrorMessages.FailedToGenerateAccessToken);

                var refreshTokenResult = await mediator.Send(new CreateRefreshTokenCommand(userId, email, roles));
                if (refreshTokenResult is { IsFailure: true })
                    return Result<AuthResponseDto>.Failure(ErrorMessages.FailedToGenerateRefreshToken);

                if (accessTokenResult.Data is null)
                    return Result<AuthResponseDto>.Failure(ErrorMessages.FailedToGenerateAccessToken);

                var authResponse = new AuthResponseDto
                {
                    AccessToken = accessTokenResult.Data.Token,
                    AccessTokenExpiration = accessTokenResult.Data.Expires,
                };

                return Result<AuthResponseDto>.Success(authResponse);
            }
            catch (Exception ex)
            {
                logService.LogError(ex, ErrorMessages.ErrorGeneratingTokens);
                return Result<AuthResponseDto>.Failure(ex.Message);
            }
        }
    }
}
