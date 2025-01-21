namespace OnlineStoreWeb.API.Repositories.Account;

public interface IAccountRepository
{
    Task<List<Model.Account>> Get();
    Task Add(Model.Account userAccount);
    Task Update(Model.Account account);
    Task Delete(Model.Account account);
}