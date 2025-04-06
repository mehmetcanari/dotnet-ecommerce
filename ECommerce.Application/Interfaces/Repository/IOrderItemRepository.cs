namespace ECommerce.Application.Interfaces.Repository;

public interface IOrderItemRepository
{
    Task Create(Domain.Model.OrderItem orderItem);
    Task Update(Domain.Model.OrderItem orderItem);
    Task Delete(Domain.Model.OrderItem orderItem);
    Task<IEnumerable<Domain.Model.OrderItem>> Read();
}