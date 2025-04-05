namespace ECommerce.API.Repositories.Order;

public interface IOrderRepository
{
    Task Create(Model.Order order);
    Task<List<Model.Order>> Read();
    Task Update(Model.Order order);
    Task Delete(Model.Order order);

    #region Filtered Queries

    Task<Model.Order> GetOrderById(int orderId);

    #endregion
}