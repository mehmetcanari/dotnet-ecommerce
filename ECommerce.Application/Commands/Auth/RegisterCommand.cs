using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Commands.Auth;

public class RegisterCommand(AccountRegisterRequestDto request) : IRequest<Result>
{
    public readonly AccountRegisterRequestDto Model = request;
}

public class RegisterCommandHandler(UserManager<Domain.Model.User> userManager, RoleManager<IdentityRole<Guid>> roleManager, IAccountRepository accountRepository, 
    ICrossContextUnitOfWork unitOfWork, ILogService logService)  : IRequestHandler<RegisterCommand, Result>
{
    private const string Role = "User";
    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var existingUserByIdentity = await userManager.Users.AnyAsync(u => u.IdentityNumber == request.Model.IdentityNumber, cancellationToken);

            if (existingUserByIdentity)
                return Result.Failure(ErrorMessages.IdentityNumberAlreadyExists);

            var user = new Domain.Model.User
            {
                UserName = request.Model.Email,
                Email = request.Model.Email,
                PhoneNumber = request.Model.PhoneNumber,
                Name = request.Model.Name,
                Surname = request.Model.Surname,
                IdentityNumber = request.Model.IdentityNumber,
                City = request.Model.City,
                Country = request.Model.Country,
                ZipCode = request.Model.ZipCode,
                Address = request.Model.Address,
                DateOfBirth = request.Model.DateOfBirth.ToUniversalTime(),
            };

            var createResult = await userManager.CreateAsync(user, request.Model.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).Aggregate((a, b) => a + ", " + b);
                await unitOfWork.RollbackTransaction();
                return Result.Failure(errors);
            }

            await accountRepository.CreateAsync(user, cancellationToken);

            var roleResult = await AssignUserRoleAsync(user);
            if (roleResult is { IsFailure: true, Message: not null })
                return Result.Failure(roleResult.Message);

            await unitOfWork.CommitTransactionAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logService.LogError(ex, ErrorMessages.AccountCreationFailed);
            await unitOfWork.RollbackTransaction();
            return Result.Failure(ErrorMessages.AccountCreationFailed);
        }
    }

    private async Task<Result> AssignUserRoleAsync(Domain.Model.User user)
    {
        if (!await roleManager.RoleExistsAsync(Role))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(Role));
            if (!roleResult.Succeeded)
            {
                return Result.Failure(ErrorMessages.ErrorCreatingRole);
            }
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, Role);
        if (!addRoleResult.Succeeded)
            return Result.Failure(ErrorMessages.ErrorAssigningRole);

        return Result.Success();
    }
}