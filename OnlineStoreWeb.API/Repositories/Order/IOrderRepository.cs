namespace OnlineStoreWeb.API.Repositories.Order;

public interface IOrderRepository
{
    Task Create(Model.Order order);
    Task<List<Model.Order>> Read();
    Task Update(Model.Order order);
    Task Delete(Model.Order order);
}