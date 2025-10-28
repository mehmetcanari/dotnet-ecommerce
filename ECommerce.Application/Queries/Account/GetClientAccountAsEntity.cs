using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Account;

public class GetClientAccountAsEntityQuery : IRequest<Result<User>>{}

public class GetClientAccountAsEntityQueryHandler(IUserRepository userRepository, ILogService logger, ICurrentUserService currentUserService) : IRequestHandler<GetClientAccountAsEntityQuery, Result<User>>
{
    public async Task<Result<User>> Handle(GetClientAccountAsEntityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var email = currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(email))
                return Result<User>.Failure(ErrorMessages.AccountEmailNotFound);

            var account = await userRepository.GetByEmail(email, cancellationToken);
            if (account is null)
                return Result<User>.Failure(ErrorMessages.AccountEmailNotFound);

            return Result<User>.Success(account);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.AccountEmailNotFound, ex.Message);
            return Result<User>.Failure(ErrorMessages.AccountEmailNotFound);
        }
    }
}