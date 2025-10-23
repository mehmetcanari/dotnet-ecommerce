using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IOrderRepository
{
    Task Create(Order order, CancellationToken cancellationToken = default);
    Task<List<Order>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<List<Order>> GetAccountPendingOrders(string userId, CancellationToken cancellationToken = default);
    Task<Order> GetOrderById(int id, CancellationToken cancellationToken = default);
    Task<Order> GetOrderByAccountId(string userId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetAccountOrders(string userId, CancellationToken cancellationToken = default);
    void Update(Order order);
    void Delete(Order order);
}