using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.Utility;
using ECommerce.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;

public class CreateIdentityUserCommand : IRequest<Result<IdentityUser>>
{
    public required AccountRegisterRequestDto AccountRegisterRequestDto { get; set; }
    public required string Role { get; set; }
}

public class CreateIdentityUserCommandHandler : IRequestHandler<CreateIdentityUserCommand, Result<IdentityUser>>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public CreateIdentityUserCommandHandler(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<IdentityUser>> Handle(CreateIdentityUserCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser
        {
            UserName = request.AccountRegisterRequestDto.Email,
            Email = request.AccountRegisterRequestDto.Email,
            PhoneNumber = request.AccountRegisterRequestDto.PhoneNumber,
        };

        var result = await _userManager.CreateAsync(user, request.AccountRegisterRequestDto.Password);
        if (!result.Succeeded)
        {
            return Result<IdentityUser>.Failure(result.Errors.Select(e => e.Description).Aggregate((a, b) => a + ", " + b));
        }

        var roleResult = await AssignUserRoleAsync(user, request.Role);
        if (roleResult is { IsFailure: true, Error: not null })
        {
            return Result<IdentityUser>.Failure(roleResult.Error);
        }

        return Result<IdentityUser>.Success(user);
    }

    private async Task<Result> AssignUserRoleAsync(IdentityUser user, string role)
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