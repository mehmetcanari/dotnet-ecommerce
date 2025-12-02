namespace ECommerce.Domain.Model;

public class WishlistItem : BaseEntity
{
    public Guid UserId { get; init; }
    public Guid ProductId { get; init; }
}