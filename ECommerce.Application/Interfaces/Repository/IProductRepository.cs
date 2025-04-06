namespace ECommerce.Application.Interfaces.Repository;

public interface IProductRepository
{
    Task Create(Domain.Model.Product product);
    Task<List<Domain.Model.Product>> Read();
    Task Update(Domain.Model.Product product);
    Task Delete(Domain.Model.Product product);
}