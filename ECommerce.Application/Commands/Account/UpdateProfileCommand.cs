using ECommerce.Application.Abstract;
using ECommerce.Application.Commands.Auth;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Commands.Account;

public class UpdateProfileCommand(AccountUpdateRequestDto request) : IRequest<Result>
{
    public readonly AccountUpdateRequestDto Model = request;
}

public class UpdateAccountCommandHandler(ICurrentUserService currentUserService, IUserRepository userRepository, UserManager<User> userManager, ICrossContextUnitOfWork unitOfWork,
    ILogService logService, IMediator mediator) : IRequestHandler<UpdateProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(ErrorMessages.UnauthorizedAction);

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            if (!string.IsNullOrEmpty(request.Model.Email) && !string.Equals(user.Email, request.Model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await userManager.FindByEmailAsync(request.Model.Email);
                if (existingUser is not null)
                    return Result.Failure(ErrorMessages.EmailAlreadyInUse);

                var emailResult = await userManager.SetEmailAsync(user, request.Model.Email);
                if (!emailResult.Succeeded)
                    return Result.Failure(ErrorMessages.AccountUpdateFailed);

                user.EmailConfirmed = false;
            }

            if (!string.IsNullOrEmpty(request.Model.PhoneNumber) && !string.Equals(user.PhoneNumber, request.Model.PhoneNumber))
            {
                var phoneResult = await userManager.SetPhoneNumberAsync(user, request.Model.PhoneNumber);
                if (!phoneResult.Succeeded)
                    return Result.Failure(ErrorMessages.AccountUpdateFailed);

                user.PhoneNumberConfirmed = false;
            }

            if (!string.IsNullOrEmpty(request.Model.Password))
            {
                if (!string.IsNullOrEmpty(request.Model.OldPassword))
                {
                    var isValidPassword = await userManager.CheckPasswordAsync(user, request.Model.OldPassword);
                    if (!isValidPassword)
                        return Result.Failure(ErrorMessages.OldPasswordIncorrect);

                    var changePasswordResult = await userManager.ChangePasswordAsync(user, request.Model.OldPassword, request.Model.Password);
                    if (!changePasswordResult.Succeeded)
                        return Result.Failure(ErrorMessages.AccountUpdateFailed);
                }
                else
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await userManager.ResetPasswordAsync(user, token, request.Model.Password);
                    if (!passwordResult.Succeeded)
                        return Result.Failure(ErrorMessages.AccountUpdateFailed);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Model.Name) || !string.IsNullOrWhiteSpace(request.Model.Surname))
            {
                var fullName = $"{user.Name}{user.Surname}"
                    .ToLowerInvariant()
                    .Replace(" ", string.Empty)
                    .Trim();

                user.UserName = fullName;
                user.NormalizedUserName = fullName.ToUpperInvariant();
            }

            user.Name = string.IsNullOrEmpty(request.Model.Name) ? user.Name : request.Model.Name;
            user.Surname = string.IsNullOrEmpty(request.Model.Surname) ? user.Surname : request.Model.Surname;
            user.City = string.IsNullOrEmpty(request.Model.City) ? user.City : request.Model.City;
            user.Country = string.IsNullOrEmpty(request.Model.Country) ? user.Country : request.Model.Country;
            user.ZipCode = string.IsNullOrEmpty(request.Model.ZipCode) ? user.ZipCode : request.Model.ZipCode;
            user.Address = string.IsNullOrEmpty(request.Model.Address) ? user.Address : request.Model.Address;
            user.PhoneCode = string.IsNullOrEmpty(request.Model.PhoneCode) ? user.PhoneCode : request.Model.PhoneCode;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return Result.Failure(ErrorMessages.AccountUpdateFailed);

            userRepository.Update(user);
            await unitOfWork.Commit();

            await mediator.Send(new LogoutCommand(), cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logService.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

}
