using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;
public interface INotificationRepository
{
    Task CreateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetAsync(Guid userId, int page = 1, int size = 50, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetUnreadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    void Delete(Guid id, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}