using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IBasketItemRepository
{
    Task Create(BasketItem basketItem, CancellationToken cancellationToken = default);
    Task<List<BasketItem>> GetActiveItems(Guid userId, CancellationToken cancellationToken = default);
    Task<BasketItem?> GetById(Guid id, CancellationToken cancellationToken = default);
    void Update(BasketItem basketItem);
    void Delete(BasketItem basketItem);
}