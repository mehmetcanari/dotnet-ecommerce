using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using Microsoft.AspNetCore.Identity;
using MediatR;
using ECommerce.Shared.Constants;

namespace ECommerce.Application.Commands.Account;

public class UpdateAccountGuidCommand : IRequest<Result<Domain.Model.Account>>
{
    public required Domain.Model.Account Account { get; set; }
    public required IdentityUser User { get; set; }
}

public class UpdateAccountGuidCommandHandler : IRequestHandler<UpdateAccountGuidCommand, Result<Domain.Model.Account>>
{
    private readonly ILoggingService _logger;

    public UpdateAccountGuidCommandHandler(IAccountRepository accountRepository, ILoggingService logger)
    {
        _logger = logger;
    }

    public Task<Result<Domain.Model.Account>> Handle(UpdateAccountGuidCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var account = request.Account;
            account.IdentityId = TryParseGuid(request.User.Id);

            return Task.FromResult(Result<Domain.Model.Account>.Success(account));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Task.FromResult(Result<Domain.Model.Account>.Failure(ex.Message));
        }
    }

    private Guid TryParseGuid(string guid)
    {
        if (Guid.TryParse(guid, out var result))
        {
            return result;
        }
        return Guid.Empty;
    }
}