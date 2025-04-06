namespace ECommerce.API.Repositories.OrderItem;

public interface IOrderItemRepository
{
    Task Create(Model.OrderItem orderItem);
    Task Update(Model.OrderItem orderItem);
    Task Delete(Model.OrderItem orderItem);
    Task<IEnumerable<Model.OrderItem>> Read();
}