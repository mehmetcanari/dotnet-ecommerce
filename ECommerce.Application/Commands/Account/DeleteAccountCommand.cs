using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Commands.Account;

public class DeleteAccountCommand : IRequest<Result>
{
    public int Id { get; set; }
}

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _logger;

    public DeleteAccountCommandHandler(IAccountRepository accountRepository, UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork, ILoggingService logger)
    {
        _accountRepository = accountRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetAccountById(request.Id);
            if (account == null)
                return Result.Failure(ErrorMessages.AccountNotFound);
            

            var user = await _userManager.FindByEmailAsync(account.Email);
            if (user == null)
                return Result.Failure(ErrorMessages.IdentityUserNotFound);
            

            _accountRepository.Delete(account);
            await _userManager.DeleteAsync(user);
            await _unitOfWork.Commit();

            _logger.LogInformation(ErrorMessages.AccountDeleted, account);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}