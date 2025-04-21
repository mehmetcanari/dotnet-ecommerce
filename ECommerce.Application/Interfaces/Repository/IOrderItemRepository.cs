using ECommerce.Domain.Model;

namespace ECommerce.Application.Interfaces.Repository;

public interface IOrderItemRepository
{
    Task Create(OrderItem orderItem);
    Task<IEnumerable<OrderItem>> Read();
    void Update(OrderItem orderItem);
    void Delete(OrderItem orderItem);
}