namespace ECommerce.Application.Interfaces.Repository;

public interface IOrderRepository
{
    Task Create(Domain.Model.Order order);
    Task<List<Domain.Model.Order>> Read();
    Task Update(Domain.Model.Order order);
    Task Delete(Domain.Model.Order order);

    #region Filtered Queries

    Task<Domain.Model.Order> GetOrderById(int orderId);

    #endregion
}