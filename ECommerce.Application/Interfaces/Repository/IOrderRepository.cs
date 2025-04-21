using ECommerce.Domain.Model;
namespace ECommerce.Application.Interfaces.Repository;

public interface IOrderRepository
{
    Task Create(Order order);
    Task<List<Order>> Read();
    void Update(Order order);
    void Delete(Order order);

    #region Filtered Queries

    Task<Order> GetOrderById(int orderId);

    #endregion
}