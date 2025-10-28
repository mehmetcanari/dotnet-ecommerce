using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Commands.Account;

public class DeleteAccountCommand(Guid id) : IRequest<Result>
{
    public readonly Guid UserId = id;
}

public class DeleteAccountCommandHandler(IUserRepository userRepository, UserManager<Domain.Model.User> userManager, IUnitOfWork unitOfWork, ILogService logger) : IRequestHandler<DeleteAccountCommand, Result>
{
    public async Task<Result> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await userRepository.GetById(request.UserId, cancellationToken);
            if (account == null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            if(account.Email is null)
                return Result.Failure(ErrorMessages.IdentityUserNotFound);

            var user = await userManager.FindByEmailAsync(account.Email);
            if (user == null)
                return Result.Failure(ErrorMessages.IdentityUserNotFound);
            

            userRepository.Delete(account);
            await userManager.DeleteAsync(user);
            await unitOfWork.Commit();

            logger.LogInformation(ErrorMessages.AccountDeleted, account);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}