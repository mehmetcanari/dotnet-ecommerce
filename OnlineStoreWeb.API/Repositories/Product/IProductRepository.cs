namespace OnlineStoreWeb.API.Repositories.Product;

public interface IProductRepository
{
    Task<List<Model.Product>> Get();
    Task Add(Model.Product product);
    Task Update(Model.Product product);
    Task Delete(Model.Product product);
}