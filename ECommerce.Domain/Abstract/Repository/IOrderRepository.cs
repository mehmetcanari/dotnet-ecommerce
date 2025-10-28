using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IOrderRepository
{
    Task Create(Order order, CancellationToken cancellationToken = default);
    Task<List<Order>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<Order?> GetPendingOrderById(Guid userId, Guid orderId, CancellationToken cancellationToken = default);
    Task<Order?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByUserId(Guid userId, Guid orderId,CancellationToken cancellationToken = default);
    Task<List<Order>> GetClientCompletedOrders(Guid userId, CancellationToken cancellationToken = default);
    void Update(Order order);
    void Delete(Order order);
}