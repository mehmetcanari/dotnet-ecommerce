namespace OnlineStoreWeb.API.Repositories.OrderItem;

public interface IOrderItemRepository
{
    Task<List<Model.OrderItem>> Get();
    Task Add(Model.OrderItem orderItem);
    Task Update(Model.OrderItem orderItem);
    Task Delete(Model.OrderItem orderItem);
}