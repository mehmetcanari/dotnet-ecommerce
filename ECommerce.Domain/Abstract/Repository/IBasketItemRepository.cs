using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IBasketItemRepository
{
    Task Create(BasketItem basketItem);
    Task<List<BasketItem>> GetNonOrderedBasketItems(Account account);
    Task<List<BasketItem>?> GetNonOrderedBasketItemIncludeSpecificProduct(int productId);
    Task<BasketItem?> GetSpecificAccountBasketItemWithId(int id, Account account);
    void Update(BasketItem basketItem);
    void Delete(BasketItem basketItem);
}