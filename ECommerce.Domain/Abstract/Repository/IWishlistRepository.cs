using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository
{
    public interface IWishlistRepository
    {
        Task Create(WishlistItem item, CancellationToken cancellationToken);
        Task<List<WishlistItem>> Read(Guid userId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
        Task<WishlistItem?> GetById(Guid userId, Guid productId, CancellationToken cancellationToken = default);
        Task Delete(WishlistItem product, CancellationToken cancellationToken = default);
        Task<bool> Exists(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    }
}
