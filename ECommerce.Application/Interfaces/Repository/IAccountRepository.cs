namespace ECommerce.Application.Interfaces.Repository;

public interface IAccountRepository
{
    Task Create(Domain.Model.Account userAccount);
    Task<List<Domain.Model.Account>> Read();
    Task Update(Domain.Model.Account account);
    Task Delete(Domain.Model.Account account);
}