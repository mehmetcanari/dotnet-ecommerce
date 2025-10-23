using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IBasketItemRepository
{
    Task Create(BasketItem basketItem, CancellationToken cancellationToken = default);
    Task<List<BasketItem>> GetNonOrderedBasketItems(User account, CancellationToken cancellationToken = default);
    Task<List<BasketItem>?> GetNonOrderedBasketItemIncludeSpecificProduct(int productId, CancellationToken cancellationToken = default);
    Task<BasketItem?> GetSpecificAccountBasketItemWithId(int id, User account, CancellationToken cancellationToken = default);
    void Update(BasketItem basketItem);
    void Delete(BasketItem basketItem);
}