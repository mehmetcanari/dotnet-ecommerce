using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Commands.Account;

public class RegisterUserCommand(AccountRegisterRequestDto request, string role) : IRequest<Result<ECommerce.Domain.Model.User>>
{
    public readonly AccountRegisterRequestDto Model = request;
    public readonly string Role = role;
}

public class RegisterUserCommandHandler(UserManager<ECommerce.Domain.Model.User> userManager, RoleManager<IdentityRole<Guid>> roleManager, IAccountRepository accountRepository)  : IRequestHandler<RegisterUserCommand, Result<ECommerce.Domain.Model.User>>
{
    public async Task<Result<ECommerce.Domain.Model.User>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUserByIdentity = await userManager.Users.AnyAsync(u => u.IdentityNumber == request.Model.IdentityNumber, cancellationToken);

        if (existingUserByIdentity)
            return Result<ECommerce.Domain.Model.User>.Failure(ErrorMessages.IdentityNumberAlreadyExists);

        var user = new ECommerce.Domain.Model.User
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

        var userManagerCreateResult = await userManager.CreateAsync(user, request.Model.Password);
        if (!userManagerCreateResult.Succeeded)
        {
            var errors = userManagerCreateResult.Errors.Select(e => e.Description).Aggregate((a, b) => a + ", " + b);
            return Result<ECommerce.Domain.Model.User>.Failure(errors);
        }

        await accountRepository.CreateAsync(user, cancellationToken);

        var roleResult = await AssignUserRoleAsync(user, request.Role);
        if (roleResult is { IsFailure: true, Message: not null })
            return Result<ECommerce.Domain.Model.User>.Failure(roleResult.Message);

        return Result<ECommerce.Domain.Model.User>.Success(user);
    }

    private async Task<Result> AssignUserRoleAsync(ECommerce.Domain.Model.User user, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            if (!roleResult.Succeeded)
            {
                return Result.Failure(ErrorMessages.ErrorCreatingRole);
            }
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, role);
        if (!addRoleResult.Succeeded)
        {
            return Result.Failure(ErrorMessages.ErrorAssigningRole);
        }

        return Result.Success();
    }
}