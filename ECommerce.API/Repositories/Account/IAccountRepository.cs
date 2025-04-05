namespace ECommerce.API.Repositories.Account;

public interface IAccountRepository
{
    Task Create(Model.Account userAccount);
    Task<List<Model.Account>> Read();
    Task Update(Model.Account account);
    Task Delete(Model.Account account);
}