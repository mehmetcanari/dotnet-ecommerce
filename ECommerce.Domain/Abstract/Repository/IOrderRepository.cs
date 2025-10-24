using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IOrderRepository
{
    Task Create(Order order, CancellationToken cancellationToken = default);
    Task<List<Order>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<List<Order>> GetPendings(Guid userId, CancellationToken cancellationToken = default);
    Task<Order> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<Order> GetByUserId(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetOrders(Guid userId, CancellationToken cancellationToken = default);
    void Update(Order order);
    void Delete(Order order);
}