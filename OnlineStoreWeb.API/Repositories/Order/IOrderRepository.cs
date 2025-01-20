public interface IOrderRepository
{
    Task<List<Order>> Get();
    Task Add(Order order);
    Task Delete(Order order);
    Task Update(Order order);
}