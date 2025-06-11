using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Queries.Account;

public class GetAccountByEmailQuery : IRequest<Result<AccountResponseDto>>
{
    public required string Email { get; set; }
}

public class GetAccountByEmailQueryHandler : IRequestHandler<GetAccountByEmailQuery, Result<AccountResponseDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;

    public GetAccountByEmailQueryHandler(
        IAccountRepository accountRepository,
        ILoggingService logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<Result<AccountResponseDto>> Handle(GetAccountByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetAccountByEmail(request.Email);
            if (account == null)
            {
                return Result<AccountResponseDto>.Failure($"User with email {request.Email} not found");
            }

            var responseDto = new AccountResponseDto
            {
                Id = account.Id,
                Name = account.Name,
                Surname = account.Surname,
                Email = account.Email,
                Address = account.Address,
                PhoneNumber = account.PhoneNumber,
                DateOfBirth = account.DateOfBirth,
                Role = account.Role
            };

            return Result<AccountResponseDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account by email: {Message}", ex.Message);
            return Result<AccountResponseDto>.Failure("An unexpected error occurred");
        }
    }
}