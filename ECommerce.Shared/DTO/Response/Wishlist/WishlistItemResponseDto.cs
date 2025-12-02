namespace ECommerce.Shared.DTO.Response.Wishlist;

public record WishlistItemResponseDto
{
    public required Guid UserId { get; set; }
    public required Guid ProductId { get; set; }
}
