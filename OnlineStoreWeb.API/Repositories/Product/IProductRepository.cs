namespace OnlineStoreWeb.API.Repositories.Product;

public interface IProductRepository
{
    Task Create(Model.Product product);
    Task<List<Model.Product>> Read();
    Task Update(Model.Product product);
    Task Delete(Model.Product product);
}