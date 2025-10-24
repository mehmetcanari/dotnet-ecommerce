using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class RegisterUserCommand : IRequest<Result<ECommerce.Domain.Model.User>>
{
    public required AccountRegisterRequestDto Model { get; set; }
    public required string Role { get; set; }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<ECommerce.Domain.Model.User>>
{
    private readonly UserManager<ECommerce.Domain.Model.User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IAccountRepository _accountRepository;

    public RegisterUserCommandHandler(UserManager<ECommerce.Domain.Model.User> userManager, RoleManager<IdentityRole<Guid>> roleManager, IAccountRepository accountRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _accountRepository = accountRepository;
    }

    public async Task<Result<ECommerce.Domain.Model.User>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUserByIdentity = await _userManager.Users.AnyAsync(u => u.IdentityNumber == request.Model.IdentityNumber, cancellationToken);

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

        var userManagerCreateResult = await _userManager.CreateAsync(user, request.Model.Password);
        if (!userManagerCreateResult.Succeeded)
        {
            var errors = userManagerCreateResult.Errors.Select(e => e.Description).Aggregate((a, b) => a + ", " + b);
            return Result<ECommerce.Domain.Model.User>.Failure(errors);
        }

        await _accountRepository.CreateAsync(user, cancellationToken);

        var roleResult = await AssignUserRoleAsync(user, request.Role);
        if (roleResult is { IsFailure: true, Message: not null })
        {
            return Result<ECommerce.Domain.Model.User>.Failure(roleResult.Message);
        }

        return Result<ECommerce.Domain.Model.User>.Success(user);
    }

    private async Task<Result> AssignUserRoleAsync(ECommerce.Domain.Model.User user, string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            if (!roleResult.Succeeded)
            {
                return Result.Failure(ErrorMessages.ErrorCreatingRole);
            }
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, role);
        if (!addRoleResult.Succeeded)
        {
            return Result.Failure(ErrorMessages.ErrorAssigningRole);
        }

        return Result.Success();
    }
}