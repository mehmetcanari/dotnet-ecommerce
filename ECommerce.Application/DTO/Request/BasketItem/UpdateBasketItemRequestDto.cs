namespace ECommerce.Application.DTO.Request.BasketItem;

public record UpdateBasketItemRequestDto
{
    public required int BasketItemId { get; set; }
    public required int Quantity { get; set; }
    public required int ProductId { get; set; }
}