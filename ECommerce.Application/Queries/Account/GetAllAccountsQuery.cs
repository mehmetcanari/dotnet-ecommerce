using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

public class GetAllAccountsQuery : IRequest<Result<List<AccountResponseDto>>>{}

public class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsQuery, Result<List<AccountResponseDto>>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;

    public GetAllAccountsQueryHandler(IAccountRepository accountRepository, ILoggingService logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<Result<List<AccountResponseDto>>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
         try
        {
            var accounts = await _accountRepository.Read();
            var accountCount = accounts.Count;
            if (accountCount == 0)
            {
                return Result<List<AccountResponseDto>>.Failure("No accounts found");
            }
            
            var accountList = accounts.Select(account => new AccountResponseDto
            {
                Id = account.Id,
                Name = account.Name,
                Surname = account.Surname,
                Email = account.Email,
                Address = account.Address,
                PhoneNumber = account.PhoneNumber,
                DateOfBirth = account.DateOfBirth,
                Role = account.Role
            }).ToList();
            
            return Result<List<AccountResponseDto>>.Success(accountList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching accounts: {Message}", ex.Message);
            return Result<List<AccountResponseDto>>.Failure("An unexpected error occurred");
        }
    }
}