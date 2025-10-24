using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IBasketItemRepository
{
    Task Create(BasketItem basketItem, CancellationToken cancellationToken = default);
    Task<List<BasketItem>> GetNonOrdereds(User account, CancellationToken cancellationToken = default);
    Task<List<BasketItem>?> GetUnorderedsByProductId(Guid productId, CancellationToken cancellationToken = default);
    Task<BasketItem?> GetUserCart(Guid id, User account, CancellationToken cancellationToken = default);
    void Update(BasketItem basketItem);
    void Delete(BasketItem basketItem);
}