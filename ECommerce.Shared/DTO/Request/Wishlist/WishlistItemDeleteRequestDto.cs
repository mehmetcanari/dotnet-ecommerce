namespace ECommerce.Shared.DTO.Request.Wishlist;

public record WishlistItemDeleteRequestDto
{
    public required Guid ProductId { get; set; }
}
