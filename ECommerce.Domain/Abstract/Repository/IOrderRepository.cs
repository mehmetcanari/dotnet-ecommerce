using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IOrderRepository
{
    Task Create(Order order);
    Task<List<Order>> Read();
    void Update(Order order);
    void Delete(Order order);
}