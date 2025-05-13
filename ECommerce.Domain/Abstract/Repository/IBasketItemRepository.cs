using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IBasketItemRepository
{
    Task Create(BasketItem basketItem);
    Task<IEnumerable<BasketItem>> Read();
    void Update(BasketItem basketItem);
    void Delete(BasketItem basketItem);
}