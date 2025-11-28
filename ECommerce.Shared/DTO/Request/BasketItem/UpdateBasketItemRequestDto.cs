namespace ECommerce.Shared.DTO.Request.BasketItem;

public record UpdateBasketItemRequestDto
{
    public required Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}