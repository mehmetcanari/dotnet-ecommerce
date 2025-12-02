namespace ECommerce.Shared.DTO.Request.Wishlist;

public record WishlistItemCreateRequestDto
{
    public required Guid ProductId { get; set; }
}