namespace ECommerce.Application.DTO.Request.BasketItem;

public record CreateBasketItemRequestDto
{
    public required Guid ProductId { get; set; }
    public required int Quantity { get; set; }
}