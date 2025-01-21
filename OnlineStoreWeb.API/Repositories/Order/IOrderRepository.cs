namespace OnlineStoreWeb.API.Repositories.Order;

public interface IOrderRepository
{
    Task<List<Model.Order>> Get();
    Task Add(Model.Order order);
    Task Delete(Model.Order order);
    Task Update(Model.Order order);
}