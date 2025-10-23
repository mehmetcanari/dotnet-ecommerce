using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class RegisterUserCommand : IRequest<Result<ECommerce.Domain.Model.User>>
{
    public required AccountRegisterRequestDto AccountRegisterRequestDto { get; set; }
    public required string Role { get; set; }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<ECommerce.Domain.Model.User>>
{
    private readonly UserManager<ECommerce.Domain.Model.User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAccountRepository _accountRepository;

    public RegisterUserCommandHandler(UserManager<ECommerce.Domain.Model.User> userManager, RoleManager<IdentityRole> roleManager, IAccountRepository accountRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _accountRepository = accountRepository;
    }

    public async Task<Result<ECommerce.Domain.Model.User>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUserByIdentity = await _userManager.Users.AnyAsync(u => u.IdentityNumber == request.AccountRegisterRequestDto.IdentityNumber, cancellationToken);

        if (existingUserByIdentity)
            return Result<ECommerce.Domain.Model.User>.Failure(ErrorMessages.IdentityNumberAlreadyExists);

        var user = new ECommerce.Domain.Model.User
        {
            UserName = request.AccountRegisterRequestDto.Email,
            Email = request.AccountRegisterRequestDto.Email,
            PhoneNumber = request.AccountRegisterRequestDto.PhoneNumber,
            Name = request.AccountRegisterRequestDto.Name,
            Surname = request.AccountRegisterRequestDto.Surname,
            IdentityNumber = request.AccountRegisterRequestDto.IdentityNumber,
            City = request.AccountRegisterRequestDto.City,
            Country = request.AccountRegisterRequestDto.Country,
            ZipCode = request.AccountRegisterRequestDto.ZipCode,
            Address = request.AccountRegisterRequestDto.Address,
            DateOfBirth = request.AccountRegisterRequestDto.DateOfBirth.ToUniversalTime(),
        };

        var userManagerCreateResult = await _userManager.CreateAsync(user, request.AccountRegisterRequestDto.Password);
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
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
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