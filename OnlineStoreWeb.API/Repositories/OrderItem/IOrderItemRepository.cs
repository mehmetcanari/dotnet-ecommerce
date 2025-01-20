public interface IOrderItemRepository
{
    Task<List<OrderItem>> Get();
    Task Add(OrderItem orderItem);
    Task Update(OrderItem orderItem);
    Task Delete(OrderItem orderItem);
}